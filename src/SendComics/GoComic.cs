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

        public override ComicLocation GetLocation(string comicContent)
        {
            var isWrongDay = comicContent.Contains("<h4 class=\"card-title\">Today's Comic from");
            if (isWrongDay)
            {
                return ComicLocation.NotPublished;
            }

            var imageMatch = Regex.Match(comicContent, @"item-comic-image"".* data-srcset=""([^ ]+) ");
            return imageMatch.Success
                ? ComicLocation.FoundAt(imageMatch.Groups[1].Value)
                : ComicLocation.NotFound;
        }
    }
}
