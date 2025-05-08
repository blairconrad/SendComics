namespace SendComics.IntegrationTests;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using FakeItEasy;
using FluentAssertions;
using global::SendComics.Services;
using SelfInitializingFakes;
using SendGrid.Helpers.Mail;
using Xunit;

public static class ComicMailBuilderTests
{
    private const string ArloAndJanisUrl = "https://featureassets.gocomics.com/assets/de774c000757013e9d47005056a9545d";
    private const string PeanutsUrl = "https://featureassets.gocomics.com/assets/3513e420073f013e9d47005056a9545d";
    private const string BlondieUrl = "https://safr.kingfeatures.com/api/img.php?e=gif&s=c&file=QmxvbmRpZS8yMDIzLzAyL0Jsb25kaWUuMjAyMzAyMjdfMTUzNi5naWY=";
    private const string RhymesWithOrangeUrl = "ttps://safr.kingfeatures.com/api/img.php?e=gif&s=c&file=Umh5bWVzV2l0aE9yYW5nZS8yMDIzLzAyL1JoeW1lc193aXRoX09yYW5nZS4yMDIzMDIyN18xNTM2LmdpZg==";
    private const string CalvinAndHobbesSundayUrl = "https://featureassets.gocomics.com/assets/b7f786c0d7ba013d93a4005056a9545d";
    private const string FirstBreakingCatNewsImageUrl = "https://featureassets.gocomics.com/assets/d3ffe4e0e8d9013d97a1005056a9545d";
    private const string SecondBreakingCatNewsImageUrl = "https://featureassets.gocomics.com/assets/d6af6220e8d9013d97a1005056a9545d";
    private const string SchlockMercenary20000612Url = "https://www.schlockmercenary.com/strip/1/0/schlock20000612.jpg?v=1443894882526";
    private const string SchlockMercenary20200724aUrl = "https://www.schlockmercenary.com/strip/7348/0/schlock20200724a.jpg?v=1701276896559";
    private const string SchlockMercenary20200724bUrl = "https://www.schlockmercenary.com/strip/7348/1/schlock20200724b.jpg?v=1701276896559";

    [Fact]
    public static void OneSubscriberTwoComics_BuildsOneMailWithBothComics()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/OneSubscriberTwoComics_BuildsOneMailWithBothComics.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("blair.conrad@gmail.com: arloandjanis, peanuts"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(ArloAndJanisUrl);
        mails[0].HtmlContent.Should().Contain(PeanutsUrl);
    }

    [Fact]
    public static void OneSubscriberOneComicTwiceAsFast_BuildsOneMailWithBothEpisodes()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/OneSubscriberOneComicTwiceAsFast_BuildsOneMailWithBothEpisodes.xml")))
        {
            var now = DateTime.Now;
            var target = new ComicMailBuilder(
                now,
                new ConfigurationParser($"blair.conrad@gmail.com: breaking-cat-news*2-20250331-{now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(FirstBreakingCatNewsImageUrl);
        mails[0].HtmlContent.Should().Contain("alt='breaking-cat-news on 31 March 2025'");
        mails[0].HtmlContent.Should().Contain(SecondBreakingCatNewsImageUrl);
        mails[0].HtmlContent.Should().Contain("alt='breaking-cat-news on 01 April 2025'");
    }

    [Fact]
    public static void OneSubscriberOneComicFiveTimesAsFastOnlyThreeNewComicsLeft_OnlyRequestsThreeComics()
    {
        var today = new DateTime(2019, 2, 19);

        var fakeComicFetcher = A.Fake<IComicFetcher>();
        var target = new ComicMailBuilder(
            today,
            new ConfigurationParser($"blair.conrad@gmail.com: breaking-cat-news*5-20190217-20190219"),
            fakeComicFetcher,
            A.Dummy<ILogger>());

        var mailMessages = target.CreateMailMessage().ToList();

        A.CallTo(() => fakeComicFetcher.GetContent(A<Uri>._)).MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public static void OneSubscriberOneComicThreeTimesAsFastWellAfterWeCaughtUp_OnlyRequestsOneComic()
    {
        var today = new DateTime(2020, 4, 10);

        var fakeComicFetcher = A.Fake<IComicFetcher>();
        var target = new ComicMailBuilder(
            today,
            new ConfigurationParser("blair.conrad@gmail.com: breaking-cat-news*3-20170327-20190328"),
            fakeComicFetcher,
            A.Dummy<ILogger>());

        var mailMessages = target.CreateMailMessage().ToList();

        A.CallTo(() => fakeComicFetcher.GetContent(A<Uri>._)).MustHaveHappened(1, Times.Exactly);
    }

    [Fact]
    public static void TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("blair.conrad@gmail.com: peanuts; anyone@mail.org: arloandjanis"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(2);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(PeanutsUrl);

        mails[1].From.Email.Should().Be("comics@blairconrad.com");
        mails[1].HtmlContent.Should().Contain(ArloAndJanisUrl);
        mails[1].Personalizations[0].Tos.Should().HaveCount(1);
        mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
    }

    [Fact]
    public static void TwoSubscribersOnSeparateLinesOneComicEach_BuildsTwoMailsEachWithOneComic()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOnSeparateLinesOneComicEach_BuildsTwoMailsEachWithOneComic.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                    blair.conrad@gmail.com: peanuts
                    anyone@mail.org: arloandjanis

                    """),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(2);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(PeanutsUrl);

        mails[1].From.Email.Should().Be("comics@blairconrad.com");
        mails[1].HtmlContent.Should().Contain(ArloAndJanisUrl);
        mails[1].Personalizations[0].Tos.Should().HaveCount(1);
        mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
    }

    [Fact]
    public static void TwoSubscribersOneWithSpaceBeforeComic_BuildsTwoMailsEachWithOneComic()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneCommentedOut_BuildsOneMailForNonCommented.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                                        blair.conrad@gmail.com: peanuts
                                        anyone@mail.org:  peanuts

                                        """),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(2);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(PeanutsUrl);

        mails[1].From.Email.Should().Be("comics@blairconrad.com");
        mails[1].Personalizations[0].Tos.Should().HaveCount(1);
        mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
        mails[1].HtmlContent.Should().Contain(PeanutsUrl);
    }

    [Fact]
    public static void TwoSubscribersOneCommentedOut_BuildsOneMailForNonCommented()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneCommentedOut_BuildsOneMailForNonCommented.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                    blair.conrad@gmail.com: peanuts
                    # anyone@mail.org: arloandjanis

                    """),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(PeanutsUrl);
    }

    [Fact]
    public static void TwoSubscribersOneEmphatic_BuildsOneMailForEmphatic()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneEmphatic_BuildsOneMailForEmphatic.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                    blair.conrad@gmail.com: peanuts
                    ! anyone@mail.org: arloandjanis

                    """),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].HtmlContent.Should().Contain(ArloAndJanisUrl);
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
    }

    [Fact]
    public static void SubscribesToComicsKingdomComics_BuildsOneMailWithBothComics()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/SubscribesToComicsKingdomComics_BuildsOneMailWithBothComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2025, 4, 2),
                new ConfigurationParser("blair.conrad@gmail.com: blondie, rhymes-with-orange"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .Contain(BlondieUrl, "it should have Blondie").And
            .Contain(RhymesWithOrangeUrl, "it should have Rhymes with Orange");
    }

    [Theory]
    [InlineData("blondie", "https://www.comicskingdom.com/blondie/2025-04-02/")]
    [InlineData("bizarro", "https://www.comicskingdom.com/bizarro/2025-04-02/")]
    [InlineData("peanuts", "https://www.gocomics.com/peanuts/2025/04/02/")]
    [InlineData("thefarside", "https://www.thefarside.com/")]
    public static void SubscribesToOneComic_QueriesFetcherWithCorrectUrl(string comic, string expectedLocation)
    {
        var fakeComicFetcher = A.Fake<IComicFetcher>();

        var target = new ComicMailBuilder(
            new DateTime(2025, 4, 02),
            new ConfigurationParser($"blair.conrad@gmail.com: {comic}"),
            fakeComicFetcher,
            A.Dummy<ILogger>());

        var mailMessages = target.CreateMailMessage().ToList();

        A.CallTo(() => fakeComicFetcher.GetContent(new Uri(expectedLocation))).MustHaveHappened();
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public static void DinosaurComicOnAWeekend_MailIndicatesComicNotPublishedToday(DayOfWeek dayOfWeek)
    {
        var dateToCheck = MostRecent(dayOfWeek);
        var fakeComicFetcher = A.Fake<IComicFetcher>();
        var target = new ComicMailBuilder(
            dateToCheck,
            new ConfigurationParser("blair.conrad@gmail.com: dinosaur-comics"),
            fakeComicFetcher,
            A.Dummy<ILogger>());

        var mails = target.CreateMailMessage().ToList();

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .NotContain("Couldn't find comic for dinosaur-comics", "it should not have looked for the comic").And
            .Contain(
                $"No published comic for dinosaur-comics on {dateToCheck.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}.",
                "it should tell the reader why there's no comic");

        A.CallTo(fakeComicFetcher).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    public static void DinosaurComicOnAWeekday_MailIncludesComic(DayOfWeek dayOfWeek)
    {
        IList<SendGridMessage> mails = null;

        var dateToCheck = MostRecent(dayOfWeek);
        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/DinosaurComicsOn" + dayOfWeek + ".xml")))
        {
            var target = new ComicMailBuilder(
                dateToCheck,
                new ConfigurationParser("blair.conrad@gmail.com: dinosaur-comics"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .NotContain("Couldn't find comic for dinosaur-comics", "it should not have looked for the comic").And
            .NotContain($"Comic dinosaur-comics wasn't published on {dateToCheck.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}.", "it should have found the comic");
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    public static void FoxtrotOnAnythingButSunday_MailIndicatesComicNotPublishedToday(DayOfWeek dayOfWeek)
    {
        var dateToCheck = MostRecent(dayOfWeek);
        var fakeComicFetcher = A.Fake<IComicFetcher>();
        var target = new ComicMailBuilder(
            dateToCheck,
            new ConfigurationParser("blair.conrad@gmail.com: foxtrot"),
            fakeComicFetcher,
            A.Dummy<ILogger>());

        var mails = target.CreateMailMessage().ToList();

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .NotContain("Couldn't find comic for foxtrot", "it should not have looked for the comic").And
            .Contain(
                $"No published comic for foxtrot on {dateToCheck.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}.",
                "it should tell the reader why there's no comic");

        A.CallTo(fakeComicFetcher).MustNotHaveHappened();
    }

    [Fact]
    public static void FoxtrotOnSunday_MailIncludesComic()
    {
        IList<SendGridMessage> mails = null;

        var dateToCheck = MostRecent(DayOfWeek.Sunday);
        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/FoxTrotOnSunday.xml")))
        {
            var target = new ComicMailBuilder(
                dateToCheck,
                new ConfigurationParser("blair.conrad@gmail.com: foxtrot"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .NotContain("Couldn't find comic for foxtrot", "it should not have looked for the comic").And
            .NotContain($"Comic foxtrot wasn't published on {dateToCheck.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}.", "it should have found the comic");
    }

    [Fact]
    public static void CalvinAndHobbesOnSunday_MailIncludesComic()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/CalvinAndHobbesOnSunday.xml")))
        {
            var dateToCheck = MostRecent(DayOfWeek.Sunday);
            var target = new ComicMailBuilder(
                dateToCheck,
                new ConfigurationParser("blair.conrad@gmail.com: calvinandhobbes"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .NotContain("Couldn't find comic for calvinandhobbes", "it should not have looked for the comic").And
            .Contain(CalvinAndHobbesSundayUrl, "it should have found the comic");
    }

    [Fact]
    public static void TwoComicsOneThrowsWhenFetched_BuildsOneMailWithOneComicOneError()
    {
        IList<SendGridMessage> mails = null;

        var fakeComicFetcher = A.Fake<IComicFetcher>();
        A.CallTo(() => fakeComicFetcher.GetContent(new Uri("http://rhymeswithorange.com/")))
            .Throws(new WebException("Bad Request"));
        A.CallTo(() => fakeComicFetcher.GetContent(new Uri("https://www.gocomics.com/arloandjanis/2025/05/08/")))
            .Returns($"""<meta property="og:image" content="{ArloAndJanisUrl}?optimizer=image&amp;width=16&amp;quality=85 16w""");

        var now = new DateTime(2025, 5, 8);
        var target = new ComicMailBuilder(
            now,
            new ConfigurationParser("blair.conrad@gmail.com: rhymeswithorange, arloandjanis"),
            fakeComicFetcher,
            A.Dummy<ILogger>());

        mails = target.CreateMailMessage().ToList();

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should().Contain(ArloAndJanisUrl);
        mails[0].HtmlContent.Should().Contain("Couldn't find comic for rhymeswithorange");
    }

    [Fact]
    public static void SubscribesToSchlockMercenary_BuildsOneMailWithOneComics()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/SubscribesToSchlockMercenary_BuildsOneMailWithOneComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2000, 06, 12),
                new ConfigurationParser("blair.conrad@gmail.com: schlockmercenary"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .Contain(SchlockMercenary20000612Url, "it should have SchlockMercenary");
    }

    [Fact]
    public static void SchlockMercenaryTwoImageDay_BuildsOneMailWithTwoComics()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/SchlockMercenaryTwoImageDay_BuildsOneMailWithTwoComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2020, 07, 24),
                new ConfigurationParser("blair.conrad@gmail.com: schlockmercenary"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should().Match(
            $"*{SchlockMercenary20200724aUrl}*{SchlockMercenary20200724bUrl}*",
            "it should have both SchlockMercenary images");
    }

    [Fact]
    public static void TheFarSideMultipleImageDay_BuildsOneMailWithMultipleComics()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TheFarSideFiveImageDay_BuildsOneMailWithFiveComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2025, 3, 26),
                new ConfigurationParser("blair.conrad@gmail.com: thefarside"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        var expectedEmailBits = new[]
        {
            "https://contentassets.amuniversal.com/assets/9a0ff2e0963101395e72005056a9545d",
            "“It’s no use. … We’ve just got to get ourselves a real damsel.”",
            "https://contentassets.amuniversal.com/assets/9eb3dcf0d7410137c8a6005056a9545d",
            "With Roger out of the way, it was Sidney’s big chance.",
        };

        mails.Should().HaveCount(1);
        mails[0].HtmlContent.Should().Match(
            '*' + string.Join('*', expectedEmailBits) + '*',
            "it should have all The Far Side images");
    }

    [Fact]
    public static void TheFarSideMultipleImageDayButOneHasNoCaption_BuildsOneMailThatLinesUpCaptions()
    {
        IList<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TheFarSideMultipleImageDayButOneHasNoCaption_BuildsOneMailThatLinesUpCaptions.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2025, 3, 26),
                new ConfigurationParser("blair.conrad@gmail.com: thefarside"),
                fakeComicFetcher.Object,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        var expectedEmailBits = new[]
        {
            "https://contentassets.amuniversal.com/assets/3639a2e0cdd40137c56b005056a9545d",
            "On the air with <i>Snake Talk",
            "https://contentassets.amuniversal.com/assets/f9f87130e4790137cd9d005056a9545d",
            "Whale dust baths",
            "https://contentassets.amuniversal.com/assets/9a0ff2e0963101395e72005056a9545d",
            "“It’s no use. … We’ve just got to get ourselves a real damsel.”",
            "https://contentassets.amuniversal.com/assets/9eb3dcf0d7410137c8a6005056a9545d",
            "With Roger out of the way, it was Sidney’s big chance.",
        };

        mails.Should().HaveCount(1);
        mails[0].HtmlContent.Should().Match(
            '*' + string.Join('*', expectedEmailBits) + '*',
            "it should have all The Far Side images");
    }

    private static DateTime MostRecent(DayOfWeek dayOfWeek)
    {
        var now = DateTime.Now;
        var offset = (int)dayOfWeek - (int)now.DayOfWeek;
        if (offset > 0)
        {
            offset -= 7;
        }

        return now.AddDays(offset);
    }
}
