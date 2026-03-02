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
    private const string BizarroUrl = "https://wp.comicskingdom.com/comicskingdom-redesign-uploads-production/2026/03/Y2tCaXphcnJvLUVORy01ODkxNjA5.jpg";
    private const string BlondieUrl = "https://wp.comicskingdom.com/comicskingdom-redesign-uploads-production/2026/03/Y2tCbG9uZGllLUVORy01ODkxNjQz.jpg";
    private const string GoComicsAssetPattern = "'https://featureassets.gocomics.com/assets/[^']+'";

    private const string FirstRhymesWithOrangeImageUrl = "https://wp.comicskingdom.com/comicskingdom-redesign-uploads-production/2026/02/Y2tSaHltZXMgd2l0aCBPcmFuZ2UtRU5HLTU5MTgzMjM.jpg";
    private const string SecondRhymesWithOrangeImageUrl = "https://wp.comicskingdom.com/comicskingdom-redesign-uploads-production/2026/02/Y2tSaHltZXMgd2l0aCBPcmFuZ2UtRU5HLTU5MTgzMjc.jpg";
    private const string TodayRhymesWithOrangeImageUrl = "https://wp.comicskingdom.com/comicskingdom-redesign-uploads-production/2026/03/Y2tSaHltZXMgd2l0aCBPcmFuZ2UtRU5HLTU4OTI3NjU.jpg";
    private const string SchlockMercenary20000612Url = "https://www.schlockmercenary.com/strip/1/0/schlock20000612.jpg?v=1443894882526";
    private const string SchlockMercenary20200724AUrl = "https://www.schlockmercenary.com/strip/7348/0/schlock20200724a.jpg?v=1701276896559";
    private const string SchlockMercenary20200724BUrl = "https://www.schlockmercenary.com/strip/7348/1/schlock20200724b.jpg?v=1701276896559";

    [Fact]
    public static void OneSubscriberTwoComics_BuildsOneMailWithBothComics()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/OneSubscriberTwoComics_BuildsOneMailWithBothComics.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("blair.conrad@gmail.com: blondie, bizarro"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(BizarroUrl);
        mails[0].HtmlContent.Should().Contain(BlondieUrl);
    }

    [Fact]
    public static void OneSubscriberOneComicTwiceAsFast_BuildsOneMailWithBothEpisodes()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/OneSubscriberOneComicTwiceAsFast_BuildsOneMailWithBothEpisodes.xml")))
        {
            var now = DateTime.Now;
            var target = new ComicMailBuilder(
                now,
                new ConfigurationParser($"blair.conrad@gmail.com: rhymes-with-orange*2-20260227-{now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(FirstRhymesWithOrangeImageUrl);
        mails[0].HtmlContent.Should().Contain("alt='rhymes-with-orange on 27 February 2026'");
        mails[0].HtmlContent.Should().Contain(SecondRhymesWithOrangeImageUrl);
        mails[0].HtmlContent.Should().Contain("alt='rhymes-with-orange on 28 February 2026'");
    }

    [Fact]
    public static void OneSubscriberOneComicFiveTimesAsFastOnlyThreeNewComicsLeft_OnlyRequestsThreeComics()
    {
        var today = new DateTime(2026, 2, 3);

        var fakeComicFetcher = A.Fake<IComicFetcher>();
        var target = new ComicMailBuilder(
            today,
            new ConfigurationParser("blair.conrad@gmail.com: rhymes-with-orange*5-20260201-20260203"),
            fakeComicFetcher,
            A.Dummy<PlaywrightBrowserService>(),
            A.Dummy<ILogger>());

        var mailMessages = target.CreateMailMessage().ToList();

        A.CallTo(() => fakeComicFetcher.GetContent(A<Uri>._)).MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public static void OneSubscriberOneComicThreeTimesAsFastWellAfterWeCaughtUp_OnlyRequestsOneComic()
    {
        var today = new DateTime(2026, 3, 1);

        var fakeComicFetcher = A.Fake<IComicFetcher>();
        var target = new ComicMailBuilder(
            today,
            new ConfigurationParser("blair.conrad@gmail.com: rhymes-with-orange*3-20170327-20190328"),
            fakeComicFetcher,
            A.Dummy<PlaywrightBrowserService>(),
            A.Dummy<ILogger>());

        var mailMessages = target.CreateMailMessage().ToList();

        A.CallTo(() => fakeComicFetcher.GetContent(A<Uri>._)).MustHaveHappened(1, Times.Exactly);
    }

    [Fact]
    public static void TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("blair.conrad@gmail.com: blondie; anyone@mail.org: rhymes-with-orange"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(2);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(BlondieUrl);

        mails[1].From.Email.Should().Be("comics@blairconrad.com");
        mails[1].HtmlContent.Should().Contain(TodayRhymesWithOrangeImageUrl);
        mails[1].Personalizations[0].Tos.Should().HaveCount(1);
        mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
    }

    [Fact]
    public static void TwoSubscribersOnSeparateLinesOneComicEach_BuildsTwoMailsEachWithOneComic()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOnSeparateLinesOneComicEach_BuildsTwoMailsEachWithOneComic.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                    blair.conrad@gmail.com: bizarro
                    anyone@mail.org: blondie

                    """),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(2);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(BizarroUrl);

        mails[1].From.Email.Should().Be("comics@blairconrad.com");
        mails[1].HtmlContent.Should().Contain(BlondieUrl);
        mails[1].Personalizations[0].Tos.Should().HaveCount(1);
        mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
    }

    [Fact]
    public static void TwoSubscribersOneWithSpaceBeforeComic_BuildsTwoMailsEachWithOneComic()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneCommentedOut_BuildsOneMailForNonCommented.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                                        blair.conrad@gmail.com: blondie
                                        anyone@mail.org:  blondie

                                        """),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(2);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(BlondieUrl);

        mails[1].From.Email.Should().Be("comics@blairconrad.com");
        mails[1].Personalizations[0].Tos.Should().HaveCount(1);
        mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
        mails[1].HtmlContent.Should().Contain(BlondieUrl);
    }

    [Fact]
    public static void TwoSubscribersOneCommentedOut_BuildsOneMailForNonCommented()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneCommentedOut_BuildsOneMailForNonCommented.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                    blair.conrad@gmail.com: blondie
                    # anyone@mail.org: bizarro

                    """),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
        mails[0].HtmlContent.Should().Contain(BlondieUrl);
    }

    [Fact]
    public static void TwoSubscribersOneEmphatic_BuildsOneMailForEmphatic()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneEmphatic_BuildsOneMailForEmphatic.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("""
                    blair.conrad@gmail.com: rhymes-with-orange
                    ! anyone@mail.org: blondie

                    """),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].From.Email.Should().Be("comics@blairconrad.com");
        mails[0].HtmlContent.Should().Contain(BlondieUrl);
        mails[0].Personalizations[0].Tos.Should().HaveCount(1);
        mails[0].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
    }

    [Fact]
    public static void SubscribesToComicsKingdomComics_BuildsOneMailWithBothComics()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/SubscribesToComicsKingdomComics_BuildsOneMailWithBothComics.xml")))
        {
            var target = new ComicMailBuilder(
                DateTime.Now,
                new ConfigurationParser("blair.conrad@gmail.com: blondie, rhymes-with-orange"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .Contain(BlondieUrl, "it should have Blondie").And
            .MatchRegex("rhymes-with-orange on .* src='https://wp.comicskingdom.com/.*.jpg", "it should have Rhymes with Orange");
    }

    [Theory]
    [InlineData("blondie", "https://www.comicskingdom.com/blondie/2025-04-02/")]
    [InlineData("bizarro", "https://www.comicskingdom.com/bizarro/2025-04-02/")]
    [InlineData("thefarside", "https://www.thefarside.com/")]
    public static void SubscribesToOneComic_QueriesFetcherWithCorrectUrl(string comic, string expectedLocation)
    {
        var fakeComicFetcher = A.Fake<IComicFetcher>();

        var target = new ComicMailBuilder(
            new DateTime(2025, 4, 02),
            new ConfigurationParser($"blair.conrad@gmail.com: {comic}"),
            fakeComicFetcher,
            A.Dummy<PlaywrightBrowserService>(),
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
            A.Dummy<PlaywrightBrowserService>(),
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
        List<SendGridMessage> mails = null;

        var dateToCheck = MostRecent(dayOfWeek);
        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/DinosaurComicsOn" + dayOfWeek + ".xml")))
        {
            var target = new ComicMailBuilder(
                dateToCheck,
                new ConfigurationParser("blair.conrad@gmail.com: dinosaur-comics"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
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
            A.Dummy<PlaywrightBrowserService>(),
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
        List<SendGridMessage> mails = null;

        var dateToCheck = MostRecent(DayOfWeek.Sunday);
        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/FoxTrotOnSunday.xml")))
        {
            var target = new ComicMailBuilder(
                dateToCheck,
                new ConfigurationParser("blair.conrad@gmail.com: foxtrot"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
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
        List<SendGridMessage> mails = null;

        var dateToCheck = MostRecent(DayOfWeek.Sunday);
        var browserService = new PlaywrightBrowserService();
        try
        {
            var target = new ComicMailBuilder(
                dateToCheck,
                new ConfigurationParser("blair.conrad@gmail.com: calvinandhobbes"),
                new WebComicFetcher(),
                browserService,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }
        finally
        {
            browserService.DisposeAsync().AsTask().Wait();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should()
            .NotContain("Couldn't find comic for calvinandhobbes", "it should have found the comic").And
            .MatchRegex(GoComicsAssetPattern, "it should have found the comic");
    }

    [Fact]
    public static void TwoComicsOneThrowsWhenFetched_BuildsOneMailWithOneComicOneError()
    {
        List<SendGridMessage> mails = null;

        var fakeComicFetcher = A.Fake<IComicFetcher>();

        // rhymeswithorange is ComicsKingdom, not GoComics, so it uses IComicFetcher
        A.CallTo(() => fakeComicFetcher.GetContent(new Uri("https://comicskingdom.com/rhymes-with-orange/2025/05/08/")))
            .Throws(new WebException("Bad Request"));

        var browserService = new PlaywrightBrowserService();
        try
        {
            var now = new DateTime(2025, 5, 8);
            var target = new ComicMailBuilder(
                now,
                new ConfigurationParser("blair.conrad@gmail.com: rhymeswithorange, arloandjanis"),
                fakeComicFetcher,
                browserService,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }
        finally
        {
            browserService.DisposeAsync().AsTask().Wait();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should().Contain(ArloAndJanisUrl);
        mails[0].HtmlContent.Should().Contain("Couldn't find comic for <a href='https://www.gocomics.com/rhymeswithorange/2025/05/08/'>rhymeswithorange on 08 May 2025</a>. Try it yourself.</article>");
    }

    [Fact]
    public static void SubscribesToSchlockMercenary_BuildsOneMailWithOneComics()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/SubscribesToSchlockMercenary_BuildsOneMailWithOneComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2000, 06, 12),
                new ConfigurationParser("blair.conrad@gmail.com: schlockmercenary"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
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
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/SchlockMercenaryTwoImageDay_BuildsOneMailWithTwoComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2020, 07, 24),
                new ConfigurationParser("blair.conrad@gmail.com: schlockmercenary"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();
        }

        mails.Should().HaveCount(1);

        mails[0].HtmlContent.Should().Match(
            $"*{SchlockMercenary20200724AUrl}*{SchlockMercenary20200724BUrl}*",
            "it should have both SchlockMercenary images");
    }

    [Fact]
    public static void TheFarSideMultipleImageDay_BuildsOneMailWithMultipleComics()
    {
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TheFarSideFiveImageDay_BuildsOneMailWithFiveComics.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2025, 3, 26),
                new ConfigurationParser("blair.conrad@gmail.com: thefarside"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
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
        List<SendGridMessage> mails = null;

        using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                   () => new WebComicFetcher(),
                   new XmlFileRecordedCallRepository("../../../RecordedCalls/TheFarSideMultipleImageDayButOneHasNoCaption_BuildsOneMailThatLinesUpCaptions.xml")))
        {
            var target = new ComicMailBuilder(
                new DateTime(2025, 3, 26),
                new ConfigurationParser("blair.conrad@gmail.com: thefarside"),
                fakeComicFetcher.Object,
                A.Dummy<PlaywrightBrowserService>(),
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
