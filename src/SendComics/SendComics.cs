namespace SendComics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using global::SendComics.Services;

    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Harmless.")]
    internal static class SendComics
    {
        public static void Main()
        {
            var log = new Logger();
            log.Info("Beginning execution");

            var configurationLocation = Environment.GetEnvironmentVariable("SubscriberConfigurationLocation");
            log.Info("Downloading configuration from " + configurationLocation + "...");
            var configurationString = DownloadConfigurationString(configurationLocation);
            log.Info("Downloaded configuration");
            var comicMailBuilder = new ComicMailBuilder(
                DateTime.Now.Date,
                new ConfigurationParser(configurationString),
                new WebComicFetcher(),
                log);

            foreach (var mailMessage in comicMailBuilder.CreateMailMessage())
            {
                log.Info(mailMessage.Subject);
            }

            log.Info("Finished execution");
        }

        private static string DownloadConfigurationString(string configurationLocation)
        {
            using (var client = new HttpClient())
            {
                return client.GetStringAsync(new Uri(configurationLocation)).Result;
            }
        }
    }
}
