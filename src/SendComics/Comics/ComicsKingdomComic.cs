namespace SendComics.Comics
{
    using System;
    using System.Text.RegularExpressions;
    using Services;

    internal class ComicsKingdomComic : Comic
    {
        private readonly string name;

        public ComicsKingdomComic(string name, IComicFetcher comicFetcher) : base(comicFetcher)
        {
            this.name = name;
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent($"https://www.comicskingdom.com/{this.name}/{now.ToString("yyyy'-'MM'-'dd")}/");
            var match = Regex.Match(comicContent, @"property=""og:image"" content=""([^""]+)""");
            return match.Success
                ? ComicLocation.FoundAt(match.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}
