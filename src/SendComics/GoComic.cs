namespace SendComics
{
    using System.Text.RegularExpressions;

    internal class GoComic: Comic
    {
        public GoComic(string url) : base(url)
        {
        }

        public override string GetImageUrl(string comicContent)
        {
            var match = Regex.Match(comicContent, @"item-comic-image"".* data-srcset=""([^ ]+) ");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}