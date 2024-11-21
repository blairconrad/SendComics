namespace SendComics.Comics
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Services;

    internal sealed class TheFarSideComic : Comic
    {
        private static readonly Uri Url = new Uri("https://www.thefarside.com/");

        public TheFarSideComic(IComicFetcher comicFetcher)
            : base(comicFetcher)
        {
        }

        public override EpisodeContent GetContent(DateTime now)
        {
            var comicContent = this.GetContent(Url);

            var imageMatches = Regex.Matches(
                comicContent,
                "img data-src=\"(https://assets.amuniversal.com/[^\"]+)\".*?<figcaption class=\"figure-caption\">(.*?)</figcaption>",
                RegexOptions.Singleline);
            if (imageMatches.Count <= 0)
            {
                return EpisodeContent.NotFound;
            }

            var imageUrls = imageMatches.Select(match => match.Groups[1].Value);
            var captions = imageMatches.Select(match => match.Groups[2].Value);
            var figures = imageUrls.Zip(captions).Select(f => new Figure(f.First) { Caption = f.Second });
            return EpisodeContent.WithFigures(figures);
        }
    }
}
