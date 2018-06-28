namespace SendComics.Comics
{
    using System;
    using System.Text.RegularExpressions;
    using Services;

    internal class GoComic : Comic
    {
        private readonly string name;

        public GoComic(string name, IComicFetcher comicFetcher) : base(comicFetcher)
        {
            this.name = name;
        }

        public override ComicLocation GetLocation(DateTime now)
        {
            var comicContent = this.GetContent($"http://www.gocomics.com/{name}/{now.ToString("yyyy'/'MM'/'dd")}/");

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
