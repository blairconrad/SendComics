namespace SendComics.Comics;

using System;
using Services;

internal abstract class Comic(IComicFetcher comicFetcher)
{
    public abstract EpisodeContent GetContent(DateTime now);

    protected string GetContent(Uri url) => comicFetcher.GetContent(url);
}
