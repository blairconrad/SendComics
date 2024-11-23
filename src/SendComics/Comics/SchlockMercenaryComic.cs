namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Services;

/// <summary>
/// A Schlock Mercenary Comic.
/// </summary>
/// <remarks>
/// Partial because the regular expressions are generated at compile-time.
/// </remarks>
internal sealed partial class SchlockMercenaryComic(IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    private const string BaseUrl = "https://www.schlockmercenary.com";

    public override EpisodeContent GetContent(DateTime now)
    {
        var comicContent = this.GetContent(
            new Uri($"{BaseUrl}/{now.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture)}"));

        var imageMatches = ImageRegex().Matches(comicContent);
        return imageMatches.Count > 0
            ? EpisodeContent.WithImages(imageMatches.Select(match => BaseUrl + match.Groups[1].Value))
            : EpisodeContent.NotFound;
    }

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""<img src="(/strip/[^/]+/[^/]+/schlock[^.]+\.jpg[^"]*)""")]
    private static partial Regex ImageRegex();
}
