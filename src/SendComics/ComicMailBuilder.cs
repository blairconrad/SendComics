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

        public IEnumerable<Mail> CreateMailMessage()
        {
            var fromEmail = new Email("comics@blairconrad.com", "Blair Conrad");
            var mailSubject = "comics " + this.now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);

            var configuration = this.configurationSource.GetConfiguration();

            var comicLocations = configuration.GetAllEpisodes(this.now).ToDictionary(e => e, this.GetComicLocation);

            foreach (var subscriber in configuration.Subscribers)
            {
                this.log.Info($"Building mail for {subscriber.Email}…");
                var mailContent = new StringBuilder("<html>\r\n<body>\r\n");

                foreach (var episode in subscriber.GetEpisodesFor(this.now))
                {
                    this.log.Info($"  Adding {episode}…");
                    WriteImage(mailContent, episode, comicLocations[episode]);
                    this.log.Info($"  Added  {episode}");
                }

                mailContent.Append("</body>\r\n</html>\r\n");
                yield return new Mail(
                    from: fromEmail,
                    subject: mailSubject,
                    to: new Email(subscriber.Email),
                    content: new Content("text/html", mailContent.ToString()));

                this.log.Info($"Built    mail for {subscriber.Email}");
            }
        }

        private static void WriteImage(StringBuilder sink, Episode episode, ComicLocation comicLocation)
        {
            if (!comicLocation.IsPublished)
            {
                sink.AppendLine($"  No published comic for {episode}.<br>");
            }
            else if (!comicLocation.WasFound)
            {
                sink.AppendLine($"  Couldn't find comic for {episode}.<br>");
            }
            else
            {
                sink.AppendLine($"  <img alt='{episode}' src='{comicLocation.Url}'><br>");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Defensive and performed on best effort basis.")]
        private ComicLocation GetComicLocation(Episode episode)
        {
            this.log.Info($"Getting image URL for {episode}…");
            try
            {
                var comic = ComicFactory.GetComic(episode.ComicName, this.comicFetcher);
                return comic.GetLocation(episode.Date);
            }
            catch (Exception e)
            {
                this.log.Error($"Caught error getting image URL for {episode}: {e}");
                return ComicLocation.NotFound;
            }
        }
    }
}
