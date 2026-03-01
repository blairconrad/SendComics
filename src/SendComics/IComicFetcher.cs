namespace SendComics;

using System;

internal interface IComicFetcher
{
    string GetContent(Uri url);
}
