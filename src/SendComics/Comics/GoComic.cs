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
internal partial class GoComic(string name, IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    public override EpisodeContent GetContent(DateTime now)
    {
        var episode = new Episode(name, now);
        var comicContent = this.GetContent(
            new Uri($"https://www.gocomics.com/{name}/{now.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture)}/"));

        var imageMatch = ImageRegex().Match(comicContent);
        return imageMatch.Success
            ? EpisodeContent.WithImage(episode, imageMatch.Groups[1].Value)
            : EpisodeContent.NotFound(episode);
    }

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""<meta property="og:image" content="(https://featureassets.gocomics.com/assets/[a-f0-9]+)""")]
    private static partial Regex ImageRegex();
}
