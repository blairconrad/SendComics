namespace SendComics;

using System;
using System.Globalization;

public class Episode
{
    public Episode(string comicName, DateTime date)
    {
        this.ComicName = comicName;
        this.Date = date;
    }

    public string ComicName { get; }

    public DateTime Date { get; }

    public override string ToString()
    {
        return this.ComicName + " on " + this.Date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
    }

    public override bool Equals(object obj) =>
        obj is Episode otherEpisode &&
        this.ComicName == otherEpisode.ComicName &&
        this.Date == otherEpisode.Date;

    public override int GetHashCode() =>
        (this.ComicName.GetHashCode(StringComparison.Ordinal) * 33) + this.Date.GetHashCode();
}
