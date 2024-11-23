namespace SendComics;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Harmless.")]
public class Configuration(params Subscriber[] subscribers)
{
    public IEnumerable<Subscriber> Subscribers { get; } = subscribers;

    public IEnumerable<Episode> GetAllEpisodes(DateTime today)
    {
        return this.Subscribers.SelectMany(s => s.GetEpisodesFor(today)).Distinct();
    }
}
