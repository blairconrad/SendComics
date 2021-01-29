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
            try
            {
                log.Info("Beginning execution");

                var configurationLocation = Environment.GetEnvironmentVariable("SubscriberConfigurationLocation");
                log.Info("Downloading configuration from " + configurationLocation + "...");
                var configurationString = new WebClient().DownloadString(configurationLocation);
                log.Info("Downloaded configuration");
                var comicMailBuilder = new ComicMailBuilder(
                    DateTime.Now.Date,
                    new ConfigurationParser(configurationString),
                    new WebComicFetcher(),
                    log);

                foreach (var mail in comicMailBuilder.CreateMailMessage())
                {
                    mails.AddAsync(mail);
                }
            }
            catch (Exception e)
            {
                log.Error("Error " + e.ToString());
            }

            log.Info("Finished execution");
        }
    }
}
