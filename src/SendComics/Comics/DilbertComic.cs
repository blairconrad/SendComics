namespace SendComics.Comics
{
    using System.Text.RegularExpressions;

    internal class DilbertComic : Comic
    {
        public DilbertComic() : base("http://www.dilbert.com/")
        {
        }

        public override ComicLocation GetLocation(string comicContent)
        {
            var match = Regex.Match(comicContent, @"img-comic"".* src=""([^""]+)""");
            return match.Success
                ? ComicLocation.FoundAt(match.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}
