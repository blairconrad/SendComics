namespace SendComics.Comics
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Services;

    internal sealed class GoComic : Comic
    {
        private readonly string name;

        public GoComic(string name, IComicFetcher comicFetcher)
            : base(comicFetcher)
        {
            this.name = name;
        }

        public override EpisodeContent GetContent(DateTime now)
        {
            var comicContent = this.GetContent(
                new Uri($"http://www.gocomics.com/{this.name}/{now.ToString("yyyy'/'MM'/'dd", CultureInfo.InvariantCulture)}/"));

            var isWrongDay = comicContent.Contains("<h4 class=\"card-title\">Today's Comic from", StringComparison.Ordinal);
            if (isWrongDay)
            {
                return EpisodeContent.NotPublished;
            }

            var imageMatch = Regex.Match(comicContent, @"item-comic-image"".* data-srcset=""([^ ]+) ");
            return imageMatch.Success
                ? EpisodeContent.FoundAt(imageMatch.Groups[1].Value)
                : EpisodeContent.NotFound;
        }
    }
}
