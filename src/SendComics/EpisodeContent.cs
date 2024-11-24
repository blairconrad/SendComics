namespace SendComics;

using System.Collections.Generic;
using System.Linq;

internal sealed class EpisodeContent
{
    private EpisodeContent()
    {
    }

    public Episode Episode { get; private init; }

    public bool IsPublished { get; private init; }

    public bool WasFound { get; private init; }

    public IEnumerable<Figure> Figures { get; private init; }

    public static EpisodeContent NotFound(Episode episode) => new() { Episode = episode, IsPublished = true };

    public static EpisodeContent NotPublished(Episode episode) => new() { Episode = episode };

    public static EpisodeContent WithImage(Episode episode, string url) => WithImages(episode, [url]);

    public static EpisodeContent WithImages(Episode episode, IEnumerable<string> urls) =>
        WithFigures(episode, urls.Select(u => new Figure(u)));

    public static EpisodeContent WithFigures(Episode episode, IEnumerable<Figure> figures) => new()
    {
        Episode = episode,
        IsPublished = true,
        WasFound = true,
        Figures = figures,
    };
}
