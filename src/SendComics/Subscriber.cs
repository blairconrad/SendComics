namespace SendComics;

using System;
using System.Collections.Generic;
using System.Linq;

public class Subscriber(string email, Subscription[] subscriptions)
{
    private IEnumerable<Subscription> subscriptions = subscriptions;

    public string Email { get; } = email;

    internal IEnumerable<Episode> GetEpisodesFor(DateTime today) =>
        this.subscriptions.SelectMany(s => s.GetEpisodesFor(today));
}
