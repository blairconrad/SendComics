namespace SendComics
{
    using System.Collections.Generic;

    internal class EpisodeContent
    {
        private EpisodeContent()
        {
        }

        public static EpisodeContent NotPublished { get; } = new EpisodeContent();

        public static EpisodeContent NotFound { get; } = new EpisodeContent { IsPublished = true };

        public bool IsPublished { get; private set; }

        public bool WasFound { get; private set; }

        public IEnumerable<string> Urls { get; private set; }

        public static EpisodeContent FoundAt(string url) => FoundAt(new[] { url });

        public static EpisodeContent FoundAt(IEnumerable<string> urls) => new()
        {
            IsPublished = true,
            WasFound = true,
            Urls = urls,
        };
    }
}
