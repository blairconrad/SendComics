namespace SendComics.Comics
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Services;

    internal class TheFarSideComic : Comic
    {
        private const string BaseUrl = "https://thefarside.com";

        public TheFarSideComic(IComicFetcher comicFetcher)
            : base(comicFetcher)
        {
        }

        public override EpisodeContent GetContent(DateTime now)
        {
            var comicContent = this.GetContent(
                new Uri($"{BaseUrl}/"));

            var imageMatches = Regex.Matches(comicContent, "img data-src=\"(https://assets.amuniversal.com/[^\"]+)\"");
            return imageMatches.Count > 0
                ? EpisodeContent.FoundAt(imageMatches.Select(match => match.Groups[1].Value))
                : EpisodeContent.NotFound;
        }
    }
}
