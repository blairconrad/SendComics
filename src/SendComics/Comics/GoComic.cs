namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Services;

/// <summary>
/// A Go Comic Comic.
/// </summary>
/// <remarks>
/// Partial because the regular expressions are generated at compile-time.
/// </remarks>
internal partial class GoComic(string name, IComicFetcher comicFetcher, PlaywrightBrowserService browserService) : Comic(comicFetcher)
{
    public override EpisodeContent GetContent(DateTime now)
    {
        var episode = new Episode(name, now);
        var uri = new Uri($"https://www.gocomics.com/{name}/{now.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture)}/");
        var comicContent = GetContentWithBrowser(uri, browserService).Result;

        var imageMatch = ImageRegex().Match(comicContent);
        return imageMatch.Success
            ? EpisodeContent.WithImage(episode, imageMatch.Groups[1].Value)
            : EpisodeContent.NotFound(episode, uri);
    }

    /// <summary>
    /// Fetches content from a URL using a headless browser.
    /// </summary>
    /// <param name="uri">The URI to fetch.</param>
    /// <param name="browserService">The shared browser service.</param>
    /// <returns>The HTML content of the page.</returns>
    private static async Task<string> GetContentWithBrowser(Uri uri, PlaywrightBrowserService browserService)
    {
        var page = await browserService.GetPageAsync().ConfigureAwait(false);
        await page.GotoAsync(uri.AbsoluteUri).ConfigureAwait(false);
        var content = await page.ContentAsync().ConfigureAwait(false);
        await page.Context.DisposeAsync().ConfigureAwait(false);
        return content;
    }

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""<meta property="og:image" content="(https://featureassets.gocomics.com/assets/[a-f0-9]+)""")]
    private static partial Regex ImageRegex();
}
