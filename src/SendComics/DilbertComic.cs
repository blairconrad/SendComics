namespace SendComics
{
    using System;
    using System.Text.RegularExpressions;

    internal class DilbertComic : Comic
    {
        public DilbertComic() : base("http://www.dilbert.com/")
        {
        }

        public override string GetImageUrl(string comicContent)
        {
            var match = Regex.Match(comicContent, @"img-comic"".* src=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}