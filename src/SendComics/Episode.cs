namespace SendComics;

using System;
using System.Globalization;

public class Episode(string comicName, DateTime date)
{
    public string ComicName { get; } = comicName;

    public DateTime Date { get; } = date;

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
