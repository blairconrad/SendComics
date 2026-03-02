namespace SendComics;

using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class Subscriber(string email, IEnumerable<Subscription> subscriptions)
{
    public string Email { get; } = email;

    internal IEnumerable<Episode> GetEpisodesFor(DateTime today) =>
        subscriptions.SelectMany(s => s.GetEpisodesFor(today));
}
