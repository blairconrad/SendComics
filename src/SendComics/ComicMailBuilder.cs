namespace SendComics;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using SendGrid.Helpers.Mail;
using Services;

public class ComicMailBuilder(
    DateTime now,
    IConfigurationSource configurationSource,
    IComicFetcher comicFetcher,
    ILogger log)
{
    public IEnumerable<SendGridMessage> CreateMailMessage()
    {
        var fromEmail = new EmailAddress("comics@blairconrad.com", "Blair Conrad");
        var mailSubject = "comics " + now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);

        var configuration = configurationSource.GetConfiguration();

        var episodesContentMap = configuration.GetAllEpisodes(now).ToDictionary(e => e, this.GetEpisodeContent);

        foreach (var (subscriber, i) in configuration.Subscribers.Select((value, i) => (value, i)))
        {
            log.Info($"Building mail for subscriber {i}…");
            var mailContent = new StringBuilder("""
                <html>
                <head>
                  <style>
                    figure {
                        margin-bottom: 1em;
                        max-width: fit-content;
                        padding-bottom: 1em;
                    }
                    img {
                      max-height: 600px;
                      max-width: 900px;
                    }
                    figcaption {
                      font-size: 150%;
                      font-style: italic;
                      text-align: center;
                    }
                  </style>
                </head>
                <body>

                """);

            foreach (var episode in subscriber.GetEpisodesFor(now))
            {
                log.Info($"  Adding {episode}…");
                WriteEpisode(mailContent, episode, episodesContentMap[episode]);
                log.Info($"  Added  {episode}");
            }

            mailContent.AppendLine("""

                </body>
                </html>
                """);

            var message = new SendGridMessage
            {
                From = fromEmail,
                Subject = mailSubject,
                HtmlContent = mailContent.ToString(),
            };
            message.AddTo(new EmailAddress(subscriber.Email));
            yield return message;

            log.Info($"Built    mail for subscriber {i}");
        }
    }

    private static void WriteEpisode(StringBuilder sink, Episode episode, EpisodeContent episodeContent)
    {
        sink.AppendLine(CultureInfo.InvariantCulture, $"<article title='{episode}'>");

        if (!episodeContent.IsPublished)
        {
            sink.Append("  No published comic for ").Append(episode).Append('.');
        }
        else if (!episodeContent.WasFound)
        {
            sink.Append("  Couldn't find comic for ").Append(episode).Append('.');
        }
        else
        {
            episodeContent.Figures.ToList()
                .ForEach(figure => WriteEpisodeImage(sink, episode, figure));
        }

        sink.AppendLine("</article>");
    }

    private static void WriteEpisodeImage(StringBuilder sink, Episode episode, Figure figure)
    {
        sink.AppendLine("  <figure>")
            .Append("    <img alt='")
            .Append(episode)
            .Append("' src='")
            .Append(figure.ImageLocation)
            .AppendLine("'>");

        if (figure.HasCaption)
        {
            sink.Append("    <figcaption>").Append(figure.Caption).AppendLine("</figcaption>");
        }

        sink
            .AppendLine("  </figure>");
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Defensive and performed on best effort basis.")]
    private EpisodeContent GetEpisodeContent(Episode episode)
    {
        log.Info($"Getting image URL for {episode}…");
        try
        {
            var comic = ComicFactory.GetComic(episode.ComicName, comicFetcher);
            return comic.GetContent(episode.Date);
        }
        catch (Exception e)
        {
            log.Error($"Caught error getting image URL for {episode}: {e}");
            return EpisodeContent.NotFound;
        }
    }
}
