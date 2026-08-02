namespace SendComics;

using System;

internal interface IComicFetcher
{
    string GetContent(Uri url, Func<string, bool> isReady);
}
