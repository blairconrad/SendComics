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

            var comicLocations = configuration.GetAllEpisodes(this.now).ToDictionary(e => e, GetComicLocation);

            foreach (var subscriber in configuration.Subscribers)
            {
                log.Info($"Building mail for {subscriber.Email}…");
                var mailContent = new StringBuilder("<html>\r\n<body>\r\n");

                foreach (var episode in subscriber.GetEpisodesFor(this.now))
                {
                    log.Info($"  Adding {episode}…");
                    WriteImage(mailContent, episode, comicLocations[episode]);
                    log.Info($"  Added  {episode}");
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
            log.Info($"Getting image URL for {episode}…");
            try
            {
                var comic = this.comicFactory.GetComic(episode.ComicName, this.comicFetcher);
                return comic.GetLocation(episode.Date);
            }
            catch (Exception e)
            {
                log.Error($"Caught error getting image URL for {episode}: {e}");
                return ComicLocation.NotFound;
            }
        }

        private void WriteImage(StringBuilder sink, Episode episode, ComicLocation comicLocation)
        {
            if (!comicLocation.IsPublished)
            {
                sink.AppendFormat("  No published comic for {0}.<br>\r\n", episode);
            }
            else if (!comicLocation.WasFound)
            {
                sink.AppendFormat("  Couldn't find comic for {0}.<br>\r\n", episode);
            }
            else
            {
                sink.AppendFormat("  <img alt='{0}' src='{1}'><br>\r\n", episode, comicLocation.Url);
            }
        }
    }
}
