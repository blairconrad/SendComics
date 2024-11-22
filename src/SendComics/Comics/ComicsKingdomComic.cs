namespace SendComics.Comics;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Services;

internal sealed class ComicsKingdomComic(string name, IComicFetcher comicFetcher) : Comic(comicFetcher)
{
    public override EpisodeContent GetContent(DateTime now)
    {
        var comicContent = this.GetContent(
            new Uri($"https://www.comicskingdom.com/{name}/{now.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture)}/"));
        var match = Regex.Match(comicContent, @"property=""og:image"" content=""([^""]+)""");
        return match.Success
            ? EpisodeContent.WithImages(match.Groups[1].Value)
            : EpisodeContent.NotFound;
    }
}
