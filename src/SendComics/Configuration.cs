namespace SendComics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Configuration
    {
        public Configuration(params Subscriber[] subscribers)
        {
            Subscribers = subscribers;
        }

        public IEnumerable<Subscriber> Subscribers { get; }

        public IEnumerable<Episode> GetAllEpisodes(DateTime today)
        {
            return Subscribers.SelectMany(s => s.GetEpisodesFor(today)).Distinct();
        }
    }
}