namespace SendComics.Comics
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Services;

    internal class ComicsKingdomComic : Comic
    {
        private readonly string name;

        public ComicsKingdomComic(string name, IComicFetcher comicFetcher)
            : base(comicFetcher)
        {
            this.name = name;
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent(
                new Uri($"https://www.comicskingdom.com/{this.name}/{now.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture)}/"));
            var match = Regex.Match(comicContent, @"property=""og:image"" content=""([^""]+)""");
            return match.Success
                ? ComicLocation.FoundAt(match.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}
