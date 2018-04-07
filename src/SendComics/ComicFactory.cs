namespace SendComics
{
    using System;

    internal class ComicFactory
    {
        public Comic GetComic(string name)
        {
            if (name == "dilbert")
            {
                return new DilbertComic("http://www.dilbert.com/");
            }

            var dateString = DateTime.Now.ToString("yyyy'/'MM'/'dd");
            return new GoComic($"http://www.gocomics.com/{name}/{dateString}/");
        }
    }
}
