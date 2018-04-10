namespace SendComics
{
    using System.Text.RegularExpressions;

    internal class KingFeatureComic : Comic
    {
        public KingFeatureComic(string url) : base(url)
        {
        }

        public override string GetImageUrl(string comicContent)
        {
            var match = Regex.Match(comicContent, @"property=""og:image"" content=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}