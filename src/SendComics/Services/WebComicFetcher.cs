namespace SendComics.Services;

using System;
using System.Net.Http;

internal sealed class WebComicFetcher : IComicFetcher
{
    public string GetContent(Uri url)
    {
        using var client = new HttpClient();
        return client.GetStringAsync(url).Result;
    }
}
