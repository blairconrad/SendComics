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

        public abstract EpisodeContent GetContent(DateTime now);

        protected string GetContent(Uri url) => this.comicFetcher.GetContent(url);
    }
}
