namespace SendComics;

using System;
using System.Collections.Generic;

public class Subscription
{
    private string comicName;
    private int comicsToDeliverPerDay;
    private DateTime? firstComicDate;
    private DateTime? subscriptionStart;

    public Subscription(string comicName)
    {
        this.comicName = comicName;
        this.comicsToDeliverPerDay = 1;
    }

    public Subscription(string comicName, int speed, DateTime firstComicDate, DateTime subscriptionStart)
    {
        this.comicName = comicName;
        this.comicsToDeliverPerDay = speed;
        this.firstComicDate = firstComicDate;
        this.subscriptionStart = subscriptionStart;
    }

    internal IEnumerable<Episode> GetEpisodesFor(DateTime today)
    {
        var effectiveSubscriptionStart = this.subscriptionStart ?? today;
        var firstComicToDeliver = this.firstComicDate ?? today;
        firstComicToDeliver = firstComicToDeliver.AddDays(this.comicsToDeliverPerDay * (today - effectiveSubscriptionStart).Days);

        DateTime episodeDate = firstComicToDeliver > today ? today : firstComicToDeliver;
        for (int i = 0; i < this.comicsToDeliverPerDay && episodeDate <= today; ++i)
        {
            yield return new Episode(this.comicName, episodeDate);
            episodeDate = episodeDate.AddDays(1);
        }
    }
}
