namespace SendComics.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses a configuration string of the form
    ///   emailaddress1: comic1a, comic1b, comic1c*2-20190101-20190430; emailaddress2: comic2a, comic2b, comic2c, …
    /// or the multi-line variant
    ///   emailaddress1: comic1a, comic1b, comic1c*2-20190101-20190430
    ///   emailaddress2: comic2a, comic2b, comic2c
    ///   …
    /// which is preferred.
    /// </summary>
    public class ConfigurationParser : IConfigurationSource
    {
        private readonly string configurationString;

        public ConfigurationParser(string configurationString)
        {
            this.configurationString = configurationString;
        }

        public Configuration GetConfiguration()
        {
            var comicSplitter = new Regex(", *");
            var subscriberSplitter = new Regex(@"; *|[\r\n]+");

            var subscribers = new List<Subscriber>();
            var subscriberStrings = subscriberSplitter
                .Split(this.configurationString)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var subscriberString in subscriberStrings)
            {
                var colonIndex = subscriberString.IndexOf(": ", StringComparison.Ordinal);
                var email = subscriberString.Substring(0, colonIndex);
                var subscriptions = comicSplitter
                    .Split(subscriberString.Substring(colonIndex + 2))
                    .Select(CreateSubscription)
                    .ToArray();

                subscribers.Add(new Subscriber(email, subscriptions));
            }

            return new Configuration(subscribers.ToArray());
        }

        private static Subscription CreateSubscription(string subscriptionString)
        {
            var nameAndAcceleration = subscriptionString.Split('*');
            var comicName = nameAndAcceleration[0];
            if (nameAndAcceleration.Length == 1)
            {
                return new Subscription(comicName);
            }

            var speedFirstComicDateAndSubscriptionStart = nameAndAcceleration[1].Split('-');
            var speed = int.Parse(speedFirstComicDateAndSubscriptionStart[0], CultureInfo.InvariantCulture);
            var firstComicDate = ParseDate(speedFirstComicDateAndSubscriptionStart[1]);
            var subscriptionStart = ParseDate(speedFirstComicDateAndSubscriptionStart[2]);

            return new Subscription(comicName, speed, firstComicDate, subscriptionStart);
        }

        private static DateTime ParseDate(string dateString) => DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
    }
}
