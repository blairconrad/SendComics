namespace SendComics
{
    using System;
    using System.Collections.Generic;
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
        private readonly ComicFactory comicFactory;

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
            this.comicFactory = new ComicFactory();
        }

        public IEnumerable<Mail> CreateMailMessage()
        {
            var fromEmail = new Email("comics@blairconrad.com", "Blair Conrad");
            var mailSubject = "comics " + this.now.ToString("dd MMMM yyyy");

            var configuration = this.configurationSource.GetConfiguration();

            var comicLocations = configuration.GetAllEpisodes(this.now).ToDictionary(e => e, e => GetComicLocation(e));

            foreach (var subscriber in configuration.Subscribers)
            {
                log.Info($"Building mail for {subscriber.Email}…");
                var mailContent = new StringBuilder("<html>\r\n<body>\r\n");

                foreach (var episode in subscriber.GetEpisodesFor(this.now))
                {
                    log.Info($"  Adding {episode.ComicName}…");
                    WriteImage(mailContent, episode.ComicName, comicLocations[episode]);
                    log.Info($"  Added  {episode.ComicName}");
                }

                mailContent.Append("</body>\r\n</html>\r\n");
                yield return new Mail(
                    from: fromEmail,
                    subject: mailSubject,
                    to: new Email(subscriber.Email),
                    content: new Content("text/html", mailContent.ToString()));

                log.Info($"Built    mail for {subscriber.Email}");
            }
        }

        private ComicLocation GetComicLocation(Episode episode)
        {
            log.Info($"Getting image URL for {episode.ComicName}…");
            var comic = this.comicFactory.GetComic(episode.ComicName, this.comicFetcher);
            return comic.GetLocation(episode.Date);
        }

        private void WriteImage(StringBuilder sink, string comicName, ComicLocation comicLocation)
        {
            if (!comicLocation.IsPublished)
            {
                sink.AppendFormat("  Comic {0} wasn't published on {1}.<br>\r\n", comicName, now.ToString("dd MMMM yyyy"));
            }
            else if (!comicLocation.WasFound)
            {
                sink.AppendFormat("  Couldn't find comic for {0}.<br>\r\n", comicName);
            }
            else
            {
                sink.AppendFormat("  <img alt='{0}' src='{1}'><br>\r\n", comicName, comicLocation.Url);
            }
        }
    }
}
