namespace SendComics.Comics
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Services;

    internal class TheFarSideComic : Comic
    {
        private const string BaseUrl = "https://thefarside.com";

        public TheFarSideComic(IComicFetcher comicFetcher)
            : base(comicFetcher)
        {
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent(
                new Uri($"{BaseUrl}/{now.ToString("yyyy'/'MM'/'dd/", CultureInfo.InvariantCulture)}"));

            var imageMatches = Regex.Matches(comicContent, "img data-src=\"(https://assets.amuniversal.com/[^\"]+)\"");
            return imageMatches.Count > 0
                ? ComicLocation.FoundAt(imageMatches.Select(match => match.Groups[1].Value))
                : ComicLocation.NotFound;
        }
    }
}
