namespace SendComics.Services
{
    using System;
    using System.Net;

    public class WebComicFetcher : IComicFetcher
    {
        public string GetContent(Uri url)
        {
            var wc = new WebClient();
            var bytes = wc.DownloadData(url);

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
