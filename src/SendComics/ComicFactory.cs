namespace SendComics
{
    using System;

    internal class ComicFactory
    {
        public Comic GetComic(string name)
        {
            if (name == "dilbert")
            {
                return new DilbertComic();
            }
            else if (name == "blondie" || name == "rhymeswithorange")
            {
                return new KingFeatureComic(name);
            }

            return new GoComic(name, DateTime.Now);
		}
	}
}
