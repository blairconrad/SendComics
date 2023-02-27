namespace SendComics.IntegrationTests
{
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
        private const string ArloAndJanisUrl = "https://assets.amuniversal.com/6d160ca0943c013be6c8005056a9545d";
        private const string ChickweedLaneUrl = "https://assets.amuniversal.com/ecd3d2c09072013be5ea005056a9545d";
        private const string BlondieUrl = "https://safr.kingfeatures.com/api/img.php?e=gif&s=c&file=QmxvbmRpZS8yMDIzLzAyL0Jsb25kaWUuMjAyMzAyMjdfMTUzNi5naWY=";
        private const string RhymesWithOrangeUrl = "ttps://safr.kingfeatures.com/api/img.php?e=gif&s=c&file=Umh5bWVzV2l0aE9yYW5nZS8yMDIzLzAyL1JoeW1lc193aXRoX09yYW5nZS4yMDIzMDIyN18xNTM2LmdpZg==";
        private const string CalvinAndHobbesSundayUrl = "https://assets.amuniversal.com/2e4e2070b4e2013816bc005056a9545d";
        private const string BreakingCatNews20170327ImageUrl = "https://assets.amuniversal.com/680049a0e683013465c3005056a9545d";
        private const string BreakingCatNews20170328ImageUrl = "https://assets.amuniversal.com/69ee7590e683013465c3005056a9545d";

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
                    new ConfigurationParser("blair.conrad@gmail.com: arloandjanis, 9chickweedlane"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].From.Email.Should().Be("comics@blairconrad.com");
            mails[0].Personalizations[0].Tos.Should().HaveCount(1);
            mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
            mails[0].HtmlContent.Should().Contain(ArloAndJanisUrl);
            mails[0].HtmlContent.Should().Contain(ChickweedLaneUrl);
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
                    new ConfigurationParser($"blair.conrad@gmail.com: breaking-cat-news*2-20170327-{now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].From.Email.Should().Be("comics@blairconrad.com");
            mails[0].Personalizations[0].Tos.Should().HaveCount(1);
            mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
            mails[0].HtmlContent.Should().Contain(BreakingCatNews20170327ImageUrl);
            mails[0].HtmlContent.Should().Contain("alt='breaking-cat-news on 27 March 2017'");
            mails[0].HtmlContent.Should().Contain(BreakingCatNews20170328ImageUrl);
            mails[0].HtmlContent.Should().Contain("alt='breaking-cat-news on 28 March 2017'");
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
                    new ConfigurationParser("blair.conrad@gmail.com: 9chickweedlane; anyone@mail.org: arloandjanis"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(2);

            mails[0].From.Email.Should().Be("comics@blairconrad.com");
            mails[0].Personalizations[0].Tos.Should().HaveCount(1);
            mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
            mails[0].HtmlContent.Should().Contain(ChickweedLaneUrl);

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
                    new ConfigurationParser(@"
blair.conrad@gmail.com: 9chickweedlane
anyone@mail.org: arloandjanis
".TrimStart()),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(2);

            mails[0].From.Email.Should().Be("comics@blairconrad.com");
            mails[0].Personalizations[0].Tos.Should().HaveCount(1);
            mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
            mails[0].HtmlContent.Should().Contain(ChickweedLaneUrl);

            mails[1].From.Email.Should().Be("comics@blairconrad.com");
            mails[1].HtmlContent.Should().Contain(ArloAndJanisUrl);
            mails[1].Personalizations[0].Tos.Should().HaveCount(1);
            mails[1].Personalizations[0].Tos[0].Email.Should().Be("anyone@mail.org");
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
                    new ConfigurationParser(@"
blair.conrad@gmail.com: 9chickweedlane
# anyone@mail.org: arloandjanis
".TrimStart()),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].From.Email.Should().Be("comics@blairconrad.com");
            mails[0].Personalizations[0].Tos.Should().HaveCount(1);
            mails[0].Personalizations[0].Tos[0].Email.Should().Be("blair.conrad@gmail.com");
            mails[0].HtmlContent.Should().Contain(ChickweedLaneUrl);
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
                    new ConfigurationParser(@"
blair.conrad@gmail.com: 9chickweedlane
! anyone@mail.org: arloandjanis
".TrimStart()),
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
                    new DateTime(2023, 2, 27),
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
        [InlineData("blondie", "https://www.comicskingdom.com/blondie/2023-02-27/")]
        [InlineData("bizarro", "https://www.comicskingdom.com/bizarro/2023-02-27/")]
        [InlineData("9chickweedlane", "http://www.gocomics.com/9chickweedlane/2023/02/27/")]
        public static void SubscribesToOneComic_QueriesFetcherWithCorrectUrl(string comic, string expectedLocation)
        {
            var fakeComicFetcher = A.Fake<IComicFetcher>();

            var target = new ComicMailBuilder(
                new DateTime(2023, 2, 27),
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
                .Contain($"No published comic for dinosaur-comics on {dateToCheck.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}.", "it should tell the reader why there's no comic");
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
            IList<SendGridMessage> mails = null;

            var dateToCheck = MostRecent(dayOfWeek);
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/FoxTrotOn" + dayOfWeek + ".xml")))
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
                .Contain($"No published comic for foxtrot on {dateToCheck.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}.", "it should tell the reader why there's no comic");
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
            A.CallTo(() => fakeComicFetcher.GetContent(new Uri("http://www.gocomics.com/arloandjanis/2023/02/27/")))
                .Returns($@"<picture class=""item-comic-image""><img class=""lazyload img - fluid"" srcset=""https://assets.gocomics.com/assets/transparent-3eb10792d1f0c7e07e7248273540f1952d9a5a2996f4b5df70ab026cd9f05517.png"" data-srcset=""{ArloAndJanisUrl} 900w"" sizes=");

            var now = new DateTime(2023, 2, 27);
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
}
