namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Services;

/// <summary>
/// A Dinosaur Comics Comic.
/// </summary>
internal sealed class DinosaurComics(IComicFetcher comicFetcher) : GoComic(Name, comicFetcher)
{
    private const string Name = "dinosaur-comics";

    public override EpisodeContent GetContent(DateTime now) =>
        now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
            ? EpisodeContent.NotPublished(new Episode(Name, now))
            : base.GetContent(now);
}
