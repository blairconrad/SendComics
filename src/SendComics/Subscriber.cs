namespace SendComics
{
    using System.Collections.Generic;

    public class Subscriber
    {
        public Subscriber(string email, params string[] subscriptions)
        {
            Email = email;
            Subscriptions = subscriptions;
        }

        public string Email { get; }
        public IEnumerable<string> Subscriptions { get; }
    }
}