namespace SendComics
{
    internal class ComicLocation
    {
        public bool IsPublished { get; private set; }

        public bool WasFound { get; private set; }

        public string Url { get; private set; }

        private ComicLocation()
        {
        }

        public static ComicLocation NotPublished { get; } = new ComicLocation();

        public static ComicLocation NotFound { get; } = new ComicLocation { IsPublished = true };

        public static ComicLocation FoundAt(string url) => new ComicLocation
        {
            IsPublished = true,
            WasFound = true,
            Url = url
        };
    }
}
