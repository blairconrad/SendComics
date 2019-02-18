namespace SendComics.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses a configuration string of the form
    /// emailaddress1: comic1a, comic1b; emailaddress2: comic2a, comic2b, comic2c, …
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
                var comics = subscriberString.Substring(colonIndex + 2);
                subscribers.Add(new Subscriber(
                    email,
                    commaSplitter.Split(comics)
                        .Select(c => new Subscription(c))
                        .ToArray()));
            }

            return new Configuration(subscribers.ToArray());
        }
    }
}