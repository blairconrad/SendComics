namespace SendComics
{
    using System;
    using global::SendComics.Services;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using SendGrid.Helpers.Mail;

    public static class SendComics
    {
        [FunctionName("SendComics")]
        public static void Run(
            [TimerTrigger("0 30 6 * * *")] TimerInfo myTimer,
            TraceWriter tracer,
            [SendGrid(ApiKey = "SendGridApiKey")] IAsyncCollector<Mail> mails)
        {
            var log = new Logger(tracer);
            log.Info("Beginning execution");

            var comicMailBuilder = new ComicMailBuilder(
                DateTime.Now,
                new SimpleConfigurationParser(Environment.GetEnvironmentVariable("SubscriberConfiguration")),
                new WebComicFetcher(),
                log);

            foreach (var mail in comicMailBuilder.CreateMailMessage())
            {
                mails.AddAsync(mail);
            }

            log.Info("Finished execution");
        }
    }
}
