namespace SendComics
{
    using System;
    using Comics;
    using Services;

    internal static class ComicFactory
    {
        public static Comic GetComic(string name, IComicFetcher comicFetcher)
        {
            if (name == "blondie" || name == "rhymes-with-orange" || name == "bizarro")
            {
                return new ComicsKingdomComic(name, comicFetcher);
            }

            if (name == "schlockmercenary")
            {
                return new SchlockMercenaryComic(comicFetcher);
            }

            return new GoComic(name, comicFetcher);
        }
    }
}
