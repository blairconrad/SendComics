namespace SendComics
{
    using Comics;
    using Services;

    internal static class ComicFactory
    {
        public static Comic GetComic(string name, IComicFetcher comicFetcher) =>
            name switch
            {
                "blondie" or "rhymes-with-orange" or "bizarro" => new ComicsKingdomComic(name, comicFetcher),
                "thefarside" => new TheFarSideComic(comicFetcher),
                "schlockmercenary" => new SchlockMercenaryComic(comicFetcher),
                _ => new GoComic(name, comicFetcher),
            };
    }
}
