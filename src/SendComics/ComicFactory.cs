namespace SendComics;

using Comics;
using Services;

internal static class ComicFactory
{
    public static Comic GetComic(string name, IComicFetcher comicFetcher, PlaywrightBrowserService browserService) =>
        name switch
        {
            "blondie" or "rhymes-with-orange" or "bizarro" => new ComicsKingdomComic(name, comicFetcher),
            "thefarside" => new TheFarSideComic(comicFetcher),
            "schlockmercenary" => new SchlockMercenaryComic(comicFetcher),
            "dinosaur-comics" => new DinosaurComics(comicFetcher, browserService),
            "foxtrot" => new FoxTrot(comicFetcher, browserService),
            _ => new GoComic(name, comicFetcher, browserService),
        };
}
