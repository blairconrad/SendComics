namespace SendComics.Services
{
    using System;

    public interface IComicFetcher
    {
        string GetContent(Uri url);
    }
}
