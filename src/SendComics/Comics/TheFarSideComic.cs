namespace SendComics.Comics;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// The Far Side Comic.
/// </summary>
/// <remarks>
/// Partial because the regular expressions are generated at compile-time.
/// </remarks>
internal sealed partial class TheFarSideComic(IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    private static readonly Uri Url = new Uri("https://www.thefarside.com/");

    public override EpisodeContent GetContent(DateTime now)
    {
        var episode = new Episode("thefarside", now);
        var comicContent = this.GetContent(Url);

        var figureMatches = FigureRegex().Matches(comicContent);

        if (figureMatches.Count <= 0)
        {
            return EpisodeContent.NotFound(episode);
        }

        var figures = new List<Figure>();
        foreach (Match figureMatch in figureMatches)
        {
            var image = ImageRegex().Match(figureMatch.Value).Groups[1].Value;
            var captionMatches = CaptionRegex().Matches(figureMatch.Value);
            var caption = captionMatches.Count > 0 ? captionMatches[0].Groups[1].Value : null;
            figures.Add(new Figure(image) { Caption = caption });
        }

        return EpisodeContent.WithFigures(episode, figures);
    }

    /// <summary>
    /// Regular expression matches the content of a figure. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex(
        """img data-src="https://\S+.amuniversal.com/[^"]+.*?<div class="card-footer""",
        RegexOptions.Singleline)]
    private static partial Regex FigureRegex();

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""
        img data-src="(https://\S+.amuniversal.com/[^"]+)
        """)]
    private static partial Regex ImageRegex();

    /// <summary>
    /// Regular expression matches an image URL. Generated at compile-time.
    /// </summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex("""<figcaption class="figure-caption">(.*?)</figcaption>""", RegexOptions.Singleline)]
    private static partial Regex CaptionRegex();
}
