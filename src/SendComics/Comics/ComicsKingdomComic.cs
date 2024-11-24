namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Services;

/// <summary>
/// A Comics Kingdom Comic.
/// </summary>
/// <remarks>
/// Partial because the regular expressions are generated at compile-time.
/// </remarks>
internal sealed partial class ComicsKingdomComic(string name, IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    public override EpisodeContent GetContent(DateTime now)
    {
        var episode = new Episode(name, now);
        var comicContent = this.GetContent(
            new Uri($"https://www.comicskingdom.com/{name}/{now.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture)}/"));
        var match = ImageRegex().Match(comicContent);
        return match.Success
            ? EpisodeContent.WithImage(episode, match.Groups[1].Value)
            : EpisodeContent.NotFound(episode);
    }

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""
        property="og:image" content="([^"]+)"
        """)]
    private static partial Regex ImageRegex();
}
