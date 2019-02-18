namespace SendComics
{
    using System.Collections.Generic;

    public class Subscriber
    {
        public Subscriber(string email, Subscription[] subscriptions)
        {
            Email = email;
            Subscriptions = subscriptions;
        }

        public string Email { get; }
        public IEnumerable<Subscription> Subscriptions { get; }
    }
}