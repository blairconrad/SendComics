namespace SendComics
{
    using System.Collections.Generic;

    internal class ComicLocation
    {
        private ComicLocation()
        {
        }

        public static ComicLocation NotPublished { get; } = new ComicLocation();

        public static ComicLocation NotFound { get; } = new ComicLocation { IsPublished = true };

        public bool IsPublished { get; private set; }

        public bool WasFound { get; private set; }

        public IEnumerable<string> Urls { get; private set; }

        public static ComicLocation FoundAt(string url) => FoundAt(new[] { url });

        public static ComicLocation FoundAt(IEnumerable<string> urls) => new()
        {
            IsPublished = true,
            WasFound = true,
            Urls = urls,
        };
    }
}
