namespace SendComics
{
    using System.Collections.Generic;

    public class Subscriber
    {
        public Subscriber(string email, params string[] comics)
        {
            Email = email;
            Comics = comics;
        }

        public string Email { get; }
        public IEnumerable<string> Comics { get; }
    }
}