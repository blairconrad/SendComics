namespace SendComics.Comics;

using System;

internal abstract class Comic(IComicFetcher comicFetcher)
{
    public abstract EpisodeContent GetContent(DateTime now);

    protected string GetContent(Uri url, Func<string, bool> isReady)
        => comicFetcher.GetContent(url, isReady);
}
