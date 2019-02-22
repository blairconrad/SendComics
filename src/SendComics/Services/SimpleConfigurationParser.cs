namespace SendComics.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses a configuration string of the form
    /// emailaddress1: comic1a, comic1b, comic1c*2-20190101-20190430; emailaddress2: comic2a, comic2b, comic2c, …
    /// </summary>
    public class SimpleConfigurationParser : IConfigurationSource
    {
        private readonly string simpleConfiguration;

        public SimpleConfigurationParser(string simpleConfiguration)
        {
            this.simpleConfiguration = simpleConfiguration;
        }

        public Configuration GetConfiguration()
        {
            var commaSplitter = new Regex(", *");
            var semicolonSplitter = new Regex("; *");

            var subscribers = new List<Subscriber>();
            var subscriberStrings = semicolonSplitter.Split(this.simpleConfiguration);
            foreach (var subscriberString in subscriberStrings)
            {
                var colonIndex = subscriberString.IndexOf(": ", StringComparison.Ordinal);
                var email = subscriberString.Substring(0, colonIndex);
                var subscriptions = commaSplitter
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
            var speed = Int32.Parse(speedFirstComicDateAndSubscriptionStart[0]);
            var firstComicDate = ParseDate(speedFirstComicDateAndSubscriptionStart[1]);
            var subscriptionStart = ParseDate(speedFirstComicDateAndSubscriptionStart[2]);

            return new Subscription(comicName, speed, firstComicDate, subscriptionStart);
        }

        private static DateTime ParseDate(string dateString) => DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
    }
}
