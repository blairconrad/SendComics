namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Services;

/// <summary>
/// A FoxTrot Comic.
/// </summary>
internal sealed class FoxTrot(IComicFetcher comicFetcher) : GoComic(Name, comicFetcher)
{
    private const string Name = "foxtrot";

    public override EpisodeContent GetContent(DateTime now) =>
        now.DayOfWeek is DayOfWeek.Sunday
            ? base.GetContent(now)
            : EpisodeContent.NotPublished(new Episode(Name, now));
}
