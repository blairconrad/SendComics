namespace SendComics
{
    using System.Collections.Generic;
    using System.Linq;

    public class Configuration
    {
        public Configuration(params Subscriber[] subscribers)
        {
            Subscribers = subscribers;
        }

        public IEnumerable<Subscriber> Subscribers { get; }

        public IEnumerable<string> GetAllComics()
        {
            return Subscribers.SelectMany(s => s.Comics).Distinct();
        }
    }
}