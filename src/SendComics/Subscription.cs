namespace SendComics
{
    using System;
    using System.Collections.Generic;

    public class Subscription
    {
        private string comicName;

        public Subscription(string comicName)
        {
            this.comicName = comicName;
        }

        internal IEnumerable<Episode> GetEpisodesFor(DateTime today) =>
            new[] { new Episode(this.comicName, today) };
    }
}