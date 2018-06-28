namespace SendComics.Comics
{
    using System;
    using System.Text.RegularExpressions;
    using Services;

    internal class KingFeatureComic : Comic
    {
        private readonly string name;

        public KingFeatureComic(string name, IComicFetcher comicFetcher) : base(comicFetcher)
        {
            this.name = name;
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent($"http://{this.name}.com/");
            var match = Regex.Match(comicContent, @"property=""og:image"" content=""([^""]+)""");
            return match.Success
                ? ComicLocation.FoundAt(match.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}
