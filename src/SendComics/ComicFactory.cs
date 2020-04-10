namespace SendComics
{
    using System;
    using Comics;
    using Services;

    internal static class ComicFactory
    {
        public static Comic GetComic(string name, IComicFetcher comicFetcher)
        {
            if (name == "dilbert")
            {
                return new DilbertComic(comicFetcher);
            }

            if (name == "blondie" || name == "rhymes-with-orange")
            {
                return new ComicsKingdomComic(name, comicFetcher);
            }

            return new GoComic(name, comicFetcher);
        }
    }
}
