namespace SendComics
{
    using System;
    using Comics;
    using Services;

    internal class ComicFactory
    {
        public Comic GetComic(string name, IComicFetcher comicFetcher)
        {
            if (name == "dilbert")
            {
                return new DilbertComic(comicFetcher);
            }

            if (name == "blondie" || name == "rhymeswithorange")
            {
                return new KingFeatureComic(name, comicFetcher);
            }

            return new GoComic(name, comicFetcher);
        }
    }
}
