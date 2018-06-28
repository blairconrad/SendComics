namespace SendComics
{
    internal abstract class Comic
    {
        protected Comic(string url)
        {
            Url = url;
        }

        public string Url { get; }

        public abstract ComicLocation GetLocation(string comicContent);
    }
}