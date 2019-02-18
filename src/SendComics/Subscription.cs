namespace SendComics
{
    public class Subscription
    {
        public string ComicName { get; }

        public Subscription(string comicName)
        {
            this.ComicName = comicName;
        }
    }
}