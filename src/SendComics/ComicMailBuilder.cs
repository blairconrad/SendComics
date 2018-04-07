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
        private readonly IConfigurationSource configurationSource;
        private readonly IComicFetcher comicFetcher;
        private readonly ILogger log;
        private readonly ComicFactory comicFactory;

        public ComicMailBuilder(
            IConfigurationSource configurationSource,
            IComicFetcher comicFetcher,
            ILogger log)
        {
            this.configurationSource = configurationSource;
            this.comicFetcher = comicFetcher;
            this.log = log;
            this.comicFactory = new ComicFactory();
        }

        public IEnumerable<Mail> CreateMailMessage()
        {
            var fromEmail = new Email("comics@blairconrad.com", "Blair Conrad");
            var mailSubject = "comics " + DateTime.Now.Date.ToString("dd MMMM yyyy");

            var configuration = this.configurationSource.GetConfiguration();

            var comicUrls = configuration.GetAllComics().ToDictionary(c => c, GetComicUrl);

            foreach (var subscriber in configuration.Subscribers)
            {
                log.Info($"Building mail for {subscriber.Email}…");
                var mailContent = new StringBuilder("<html>\r\n<body>\r\n");

                foreach (var comicName in subscriber.Comics)
                {
                    log.Info($"  Adding {comicName}…");
                    WriteImage(mailContent, comicName, comicUrls[comicName]);
                    log.Info($"  Added  {comicName}");
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

        private string GetComicUrl(string comicName)
        {
            log.Info($"Getting image URL for {comicName}…");
            var comic = this.comicFactory.GetComic(comicName);
            var comicContent = this.comicFetcher.GetContent(comic.Url);
            log.Info($"Got     image URL for {comicName}");
            return comic.GetImageUrl(comicContent);
        }

        private void WriteImage(StringBuilder sink, string comicName, string imageUrl)
        {
            if (imageUrl == null)
            {
                sink.AppendFormat("  Couldn't find comic for {0}.<br>\r\n", comicName);
            }
            else
            {
                sink.AppendFormat("  <img alt='{0}' src='{1}'><br>\r\n", comicName, imageUrl);
            }
        }
    }
}
