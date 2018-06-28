namespace SendComics
{
    using System.Text.RegularExpressions;

    internal class KingFeatureComic : Comic
    {
        public KingFeatureComic(string name) : base("http://" + name + ".com/")
        {
        }

        public override ComicLocation GetLocation(string comicContent)
        {
            var match = Regex.Match(comicContent, @"property=""og:image"" content=""([^""]+)""");
            return match.Success
                ? ComicLocation.FoundAt(match.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}