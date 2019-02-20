using System;

namespace SendComics
{
    public class Episode
    {
        public string ComicName { get; }
        public DateTime Date { get; }

        public Episode(string comicName, DateTime date)
        {
            this.ComicName = comicName;
            this.Date = date;
        }

        public override bool Equals(object otherObject) =>
            otherObject is Episode otherEpisode &&
                this.ComicName == otherEpisode.ComicName &&
                this.Date == otherEpisode.Date;

        public override int GetHashCode() =>
           this.ComicName.GetHashCode() * 33 + this.Date.GetHashCode();
    }
}