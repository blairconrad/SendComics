namespace SendComics.Services
{
    public interface IComicFetcher
    {
        string GetContent(string url);
    }
}