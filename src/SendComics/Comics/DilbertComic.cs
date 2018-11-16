namespace SendComics.Comics
{
    using System;
    using System.Text.RegularExpressions;
    using Services;

    internal class DilbertComic : Comic
    {
        public DilbertComic(IComicFetcher comicFetcher) : base(comicFetcher)
        {
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent("http://www.dilbert.com/");
            var match = Regex.Match(comicContent, @"img-comic"".* src=""([^""]+)""");
            return match.Success
                ? ComicLocation.FoundAt("https:" + match.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}
