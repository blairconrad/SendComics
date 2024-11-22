namespace SendComics;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Harmless.")]
public class Configuration
{
    public Configuration(params Subscriber[] subscribers)
    {
        this.Subscribers = subscribers;
    }

    public IEnumerable<Subscriber> Subscribers { get; }

    public IEnumerable<Episode> GetAllEpisodes(DateTime today)
    {
        return this.Subscribers.SelectMany(s => s.GetEpisodesFor(today)).Distinct();
    }
}
