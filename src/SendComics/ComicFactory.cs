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
            else if (name == "blondie" || name == "rhymeswithorange")
            {
                return new KingFeatureComic("http://" + name + ".com/");
            }

            var dateString = DateTime.Now.ToString("yyyy'/'MM'/'dd");
            return new GoComic($"http://www.gocomics.com/{name}/{dateString}/");
        }
    }
}
