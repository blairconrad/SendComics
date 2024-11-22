namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Services;

internal sealed class SchlockMercenaryComic(IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    private const string BaseUrl = "https://www.schlockmercenary.com";

    public override EpisodeContent GetContent(DateTime now)
    {
        var comicContent = this.GetContent(
            new Uri($"{BaseUrl}/{now.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture)}"));

        var imageMatches = Regex.Matches(comicContent, @"<img src=""(/strip/[^/]+/[^/]+/schlock[^.]+\.jpg[^""]*)");
        return imageMatches.Count > 0
            ? EpisodeContent.WithImages(imageMatches.Select(match => BaseUrl + match.Groups[1].Value))
            : EpisodeContent.NotFound;
    }
}
