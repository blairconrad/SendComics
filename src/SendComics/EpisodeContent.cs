namespace SendComics
{
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class EpisodeContent
    {
        private EpisodeContent()
        {
        }

        public static EpisodeContent NotPublished { get; } = new();

        public static EpisodeContent NotFound { get; } = new() { IsPublished = true };

        public bool IsPublished { get; private init; }

        public bool WasFound { get; private init; }

        public IEnumerable<Figure> Figures { get; init; }

        public static EpisodeContent WithImages(string url) => WithImages(new[] { url });

        public static EpisodeContent WithImages(IEnumerable<string> urls)
        {
            return WithFigures(urls.Select(u => new Figure(u)));
        }

        public static EpisodeContent WithFigures(IEnumerable<Figure> figures) => new()
        {
            IsPublished = true,
            WasFound = true,
            Figures = figures,
        };
    }
}
