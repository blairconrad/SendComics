namespace SendComics;

using System;

public interface IComicFetcher
{
    string GetContent(Uri url);
}
