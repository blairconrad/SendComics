namespace SendComics;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using SendGrid.Helpers.Mail;

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
                    .comics {
                      background-color: lightgoldenrodyellow;
                    }

                    article {
                      clear: both;
                      text-align: center;
                    }

                    img {
                      margin: auto;
                      max-height: 800px;
                      max-width: 100%;
                    }

                    figure {
                      background-color: white;
                      box-sizing: border-box;
                      display: inline-block;
                      margin-left: 1em;
                      margin-right: 1em;
                      padding: 1em;
                    }

                    figcaption {
                      font-size: 150%;
                      font-style: italic;
                      margin-left: auto;
                      margin-right: auto;
                      max-width: 50ch;
                    }
                  </style>
                </head>
                <body>
                <section class="comics">
                """);

            var episodeContentsForNow = subscriber
                .GetEpisodesFor(now)
                .Select(e => episodesContentMap[e])
                .ToList();

            foreach (var episodeContent in episodeContentsForNow.Where(e => e.IsPublished))
            {
                log.Info($"  Adding {episodeContent.Episode}…");
                WriteEpisode(mailContent, episodeContent);
                log.Info($"  Added  {episodeContent.Episode}");
            }

            foreach (var episodeContent in episodeContentsForNow.Where(e => !e.IsPublished))
            {
                log.Info($"  Adding {episodeContent.Episode}…");
                WriteEpisode(mailContent, episodeContent);
                log.Info($"  Added  {episodeContent.Episode}");
            }

            mailContent.AppendLine("""

                </section>
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

    private static void WriteEpisode(StringBuilder sink, EpisodeContent episodeContent)
    {
        sink.AppendLine(CultureInfo.InvariantCulture, $"<article title='{episodeContent.Episode}'>");

        if (!episodeContent.IsPublished)
        {
            sink.Append("  No published comic for ").Append(episodeContent.Episode).Append('.');
        }
        else if (!episodeContent.WasFound)
        {
            sink.Append("  Couldn't find comic for ").Append(episodeContent.Episode).Append('.');
        }
        else
        {
            episodeContent.Figures.ToList()
                .ForEach(figure => WriteEpisodeImage(sink, episodeContent.Episode, figure));
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
        log.Info($"Getting content for {episode}…");
        try
        {
            var comic = ComicFactory.GetComic(episode.ComicName, comicFetcher);
            return comic.GetContent(episode.Date);
        }
        catch (Exception e)
        {
            log.Error($"Caught error getting content for {episode}: {e}");
            return EpisodeContent.NotFound(episode);
        }
    }
}
