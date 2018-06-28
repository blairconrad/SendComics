namespace SendComics.Comics
{
    using System;
    using Services;

    internal abstract class Comic
    {
        private readonly IComicFetcher comicFetcher;

        protected Comic(IComicFetcher comicFetcher)
        {
            this.comicFetcher = comicFetcher;
        }

        protected string GetContent(string url) => this.comicFetcher.GetContent(url);

        public abstract ComicLocation GetLocation(DateTime now);
    }
}
