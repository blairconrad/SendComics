namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Services;

/// <summary>
/// A Go Comic Comic.
/// </summary>
/// <remarks>
/// Partial because the regular expressions are generated at compile-time.
/// </remarks>
internal sealed partial class GoComic(string name, IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    public override EpisodeContent GetContent(DateTime now)
    {
        var comicContent = this.GetContent(
            new Uri($"http://www.gocomics.com/{name}/{now.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture)}/"));

        var isWrongDay = comicContent.Contains("<h4 class=\"card-title\">Today's Comic from", StringComparison.Ordinal);
        if (isWrongDay)
        {
            return EpisodeContent.NotPublished;
        }

        var imageMatch = ImageRegex().Match(comicContent);
        return imageMatch.Success
            ? EpisodeContent.WithImages(imageMatch.Groups[1].Value)
            : EpisodeContent.NotFound;
    }

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""item-comic-image".* data-srcset="([^ ]+) """)]
    private static partial Regex ImageRegex();
}
