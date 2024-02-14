namespace SendComics.Comics
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Services;

    internal class SchlockMercenaryComic : Comic
    {
        private const string BaseUrl = "https://www.schlockmercenary.com";

        public SchlockMercenaryComic(IComicFetcher comicFetcher)
            : base(comicFetcher)
        {
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent(
                new Uri($"{BaseUrl}/{now.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture)}"));

            var imageMatches = Regex.Matches(comicContent, @"<img src=""(/strip/[^/]+/[^/]+/schlock[^.]+\.jpg[^""]*)");
            return imageMatches.Count > 0
                ? ComicLocation.FoundAt(imageMatches.Select(match => BaseUrl + match.Groups[1].Value))
                : ComicLocation.NotFound;
        }
    }
}
