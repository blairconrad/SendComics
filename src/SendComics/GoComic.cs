namespace SendComics
{
    using System;
    using System.Text.RegularExpressions;

    internal class GoComic : Comic
    {
        public GoComic(string name, DateTime now)
            : base($"http://www.gocomics.com/{name}/{now.ToString("yyyy'/'MM'/'dd")}/")
        {
        }

        public override string GetImageUrl(string comicContent)
        {
            var match = Regex.Match(comicContent, @"item-comic-image"".* data-srcset=""([^ ]+) ");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}