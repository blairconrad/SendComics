namespace SendComics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using global::SendComics.Services;
    using SendGrid.Helpers.Mail;

    public class ComicMailBuilder
    {
        private readonly DateTime now;
        private readonly IConfigurationSource configurationSource;
        private readonly IComicFetcher comicFetcher;
        private readonly ILogger log;

        public ComicMailBuilder(
            DateTime now,
            IConfigurationSource configurationSource,
            IComicFetcher comicFetcher,
            ILogger log)
        {
            this.now = now;
            this.configurationSource = configurationSource;
            this.comicFetcher = comicFetcher;
            this.log = log;
        }

        public IEnumerable<SendGridMessage> CreateMailMessage()
        {
            var fromEmail = new EmailAddress("comics@blairconrad.com", "Blair Conrad");
            var mailSubject = "comics " + this.now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);

            var configuration = this.configurationSource.GetConfiguration();

            var episodesContentMap = configuration.GetAllEpisodes(this.now).ToDictionary(e => e, this.GetEpisodeContent);

            foreach (var (subscriber, i) in configuration.Subscribers.Select((value, i) => (value, i)))
            {
                this.log.Info($"Building mail for subscriber {i}…");
                var mailContent = new StringBuilder();

                foreach (var episode in subscriber.GetEpisodesFor(this.now))
                {
                    this.log.Info($"  Adding {episode}…");
                    WriteEpisode(mailContent, episode, episodesContentMap[episode]);
                    this.log.Info($"  Added  {episode}");
                }

                var message = new SendGridMessage
                {
                    From = fromEmail,
                    Subject = mailSubject,
                    HtmlContent = mailContent.ToString(),
                };
                message.AddTo(new EmailAddress(subscriber.Email));
                yield return message;

                this.log.Info($"Built    mail for subscriber {i}");
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
                foreach (var url in episodeContent.Urls)
                {
                    sink.AppendLine("  <figure>")
                        .Append("    <img alt='")
                        .Append(episode)
                        .Append("' src='")
                        .Append(url)
                        .AppendLine("'>")
                        .AppendLine("  </figure>");
                }
            }

            sink.AppendLine("</article>");
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Defensive and performed on best effort basis.")]
        private EpisodeContent GetEpisodeContent(Episode episode)
        {
            this.log.Info($"Getting image URL for {episode}…");
            try
            {
                var comic = ComicFactory.GetComic(episode.ComicName, this.comicFetcher);
                return comic.GetContent(episode.Date);
            }
            catch (Exception e)
            {
                this.log.Error($"Caught error getting image URL for {episode}: {e}");
                return EpisodeContent.NotFound;
            }
        }
    }
}
