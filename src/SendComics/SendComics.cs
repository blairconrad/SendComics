namespace SendComics;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using dotenv.net;
using global::SendComics.Services;

[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Harmless.")]
internal static class SendComics
{
    public static void Main(string[] args)
    {
        var log = new Logger();
        log.Info("Beginning execution");

        DotEnv.Fluent().Load();

        var configurationLocation = Environment.GetEnvironmentVariable("SUBSCRIBER_CONFIGURATION_LOCATION");
        log.Info("Downloading configuration from " + configurationLocation + "...");
        var configurationString = DownloadConfigurationString(configurationLocation);
        log.Info("Downloaded configuration");
        var date = (args.Length > 0)
            ? DateTime.Parse(args[0], CultureInfo.InvariantCulture)
            : DateTime.Now.Date;

        var comicMailBuilder = new ComicMailBuilder(
            date,
            new ConfigurationParser(configurationString),
            new WebComicFetcher(),
            log);

        var mailer = new SendGridMailer();
        foreach (var mailMessage in comicMailBuilder.CreateMailMessage())
        {
            mailer.SendEmailAsync(mailMessage).Wait();
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
