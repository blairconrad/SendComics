﻿namespace SendComics.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Parses a configuration string of the form
///   emailaddress1: comic1a, comic1b, comic1c*2-20190101-20190430; emailaddress2: comic2a, comic2b, comic2c, …
/// or the multi-line variant
///   emailaddress1: comic1a, comic1b, comic1c*2-20190101-20190430
///   emailaddress2: comic2a, comic2b, comic2c
///   …
/// which is preferred.
/// In the multi-line format,
///   - lines beginning with # are comments and are ignored, and
///   - lines beginning with ! are "emphatic" - if any of these are present, other !-less subscribers are skipped.
/// </summary>
/// <remarks>
/// Partial because the regular expressions are generated at compile-time.
/// </remarks>
public partial class ConfigurationParser(string configurationString) : IConfigurationSource
{
    public Configuration GetConfiguration()
    {
        var comicSplitter = ComicSplitterRegex();
        var subscriberSplitter = SubscriberSplitterRegex();
        var emphaticPattern = EmphaticPatternRegex();

        IEnumerable<string> subscriberStrings = subscriberSplitter
            .Split(configurationString)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Where(s => !s.StartsWith('#'))
            .ToList();

        if (subscriberStrings.Any(emphaticPattern.IsMatch))
        {
            subscriberStrings = subscriberStrings
                .Where(s => emphaticPattern.IsMatch(s))
                .Select(s => emphaticPattern.Replace(s, string.Empty));
        }

        var subscribers = new List<Subscriber>();
        foreach (var subscriberString in subscriberStrings)
        {
            var colonIndex = subscriberString.IndexOf(": ", StringComparison.Ordinal);
            var email = subscriberString.Substring(0, colonIndex);
            var subscriptions = comicSplitter
                .Split(subscriberString.Substring(colonIndex + 2).Trim())
                .Select(CreateSubscription)
                .ToArray();

            subscribers.Add(new Subscriber(email, subscriptions));
        }

        return new Configuration(subscribers.ToArray());
    }

    private static Subscription CreateSubscription(string subscriptionString)
    {
        var nameAndAcceleration = subscriptionString.Trim().Split('*');
        var comicName = nameAndAcceleration[0];
        if (nameAndAcceleration.Length == 1)
        {
            return new Subscription(comicName);
        }

        var speedFirstComicDateAndSubscriptionStart = nameAndAcceleration[1].Split('-');
        var speed = int.Parse(speedFirstComicDateAndSubscriptionStart[0], CultureInfo.InvariantCulture);
        var firstComicDate = ParseDate(speedFirstComicDateAndSubscriptionStart[1]);
        var subscriptionStart = ParseDate(speedFirstComicDateAndSubscriptionStart[2]);

        return new Subscription(comicName, speed, firstComicDate, subscriptionStart);
    }

    private static DateTime ParseDate(string dateString) => DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture).Date;

    [GeneratedRegex(", *")]
    private static partial Regex ComicSplitterRegex();

    [GeneratedRegex(@"; *|[\r\n]+")]
    private static partial Regex SubscriberSplitterRegex();

    [GeneratedRegex("^! *")]
    private static partial Regex EmphaticPatternRegex();
}
