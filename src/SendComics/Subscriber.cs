namespace SendComics;

using System;
using System.Collections.Generic;
using System.Linq;

public class Subscriber
{
    private IEnumerable<Subscription> subscriptions;

    public Subscriber(string email, Subscription[] subscriptions)
    {
        this.Email = email;
        this.subscriptions = subscriptions;
    }

    public string Email { get; }

    internal IEnumerable<Episode> GetEpisodesFor(DateTime today) =>
        this.subscriptions.SelectMany(s => s.GetEpisodesFor(today));
}
