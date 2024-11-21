namespace SendComics
{
    using System.Collections.Generic;

    internal sealed class DayOfComic
    {
        private DayOfComic()
        {
        }

        public static DayOfComic NotPublished { get; } = new DayOfComic();

        public static DayOfComic NotFound { get; } = new DayOfComic { IsPublished = true };

        public bool IsPublished { get; private set; }

        public bool WasFound { get; private set; }

        public IEnumerable<string> Urls { get; private set; }

        public static DayOfComic FoundAt(string url) => FoundAt(new[] { url });

        public static DayOfComic FoundAt(IEnumerable<string> urls) => new()
        {
            IsPublished = true,
            WasFound = true,
            Urls = urls,
        };
    }
}
