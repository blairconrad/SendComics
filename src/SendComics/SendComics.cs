namespace SendComics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using global::SendComics.Services;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using SendGrid.Helpers.Mail;

    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Harmless, and Azure knows about the name now.")]
    public static class SendComics
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "myTimer", Justification = "Used implicitly by Azure Functions.")]
        [FunctionName("SendComics")]
        public static void Run(
            [TimerTrigger("0 30 6 * * *")] TimerInfo myTimer,
            TraceWriter tracer,
            [SendGrid(ApiKey = "SendGridApiKey")] IAsyncCollector<Mail> mails)
        {
            var log = new Logger(tracer);
            log.Info("Beginning execution");

            var configurationString = new WebClient().DownloadString(Environment.GetEnvironmentVariable("SubscriberConfigurationLocation"));
            var comicMailBuilder = new ComicMailBuilder(
                DateTime.Now.Date,
                new ConfigurationParser(configurationString),
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
