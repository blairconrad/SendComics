namespace SendComics.Services;

using System;
using System.Net;
using System.Net.Http;

public class WebComicFetcher : IComicFetcher
{
    public string GetContent(Uri url)
    {
        using (var client = new HttpClient())
        {
            return client.GetStringAsync(url).Result;
        }
    }
}
