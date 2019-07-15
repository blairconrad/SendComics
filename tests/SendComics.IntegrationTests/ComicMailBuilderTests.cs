namespace SendComics.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using FakeItEasy;
    using FluentAssertions;
    using SelfInitializingFakes;
    using global::SendComics.Services;
    using SendGrid.Helpers.Mail;
    using Xunit;

    public class ComicMailBuilderTests
    {
        private const string DilbertImageUrl = "https://assets.amuniversal.com/cfa39b00b39601365f19005056a9545d";
        private const string ChickweedLaneUrl = "https://assets.amuniversal.com/e2a3c500c015013663ff005056a9545d";
        private const string BlondieUrl = "https://safr.kingfeatures.com/api/img.php?e=png&s=c&file=QmxvbmRpZS8yMDE5LzA3L0Jsb25kaWVfaHMuMjAxOTA3MTRfMTUzNi5wbmc=";
        private const string RhymesWithOrangeUrl = "https://safr.kingfeatures.com/api/img.php?e=png&s=c&file=Umh5bWVzV2l0aE9yYW5nZS8yMDE5LzA3L1JoeW1lc193aXRoX09yYW5nZV9udGIuMjAxOTA3MTRfMTUzNi5wbmc=";
        private const string CalvinAndHobbesSundayUrl = "https://assets.amuniversal.com/65839a905f980136408e005056a9545d";
        private const string BreakingCatNews20170327ImageUrl = "https://assets.amuniversal.com/680049a0e683013465c3005056a9545d";
        private const string BreakingCatNews20170328ImageUrl = "https://assets.amuniversal.com/69ee7590e683013465c3005056a9545d";

        [Fact]
        public void OneSubscriberTwoComics_BuildsOneMailWithBothComics()
        {
            IList<Mail> mails = null;

            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/OneSubscriberTwoComics_BuildsOneMailWithBothComics.xml")))
            {
                var target = new ComicMailBuilder(
                    DateTime.Now,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: dilbert, 9chickweedlane"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].From.Address.Should().Be("comics@blairconrad.com");
            mails[0].Personalization[0].Tos.Should().HaveCount(1);
            mails[0].Personalization[0].Tos[0].Address.Should().Be("blair.conrad@gmail.com");
            mails[0].Contents[0].Value.Should().Contain(DilbertImageUrl);
            mails[0].Contents[0].Value.Should().Contain(ChickweedLaneUrl);
        }

        [Fact]
        public void OneSubscriberOneComicTwiceAsFast_BuildsOneMailWithBothEpisodes()
        {
            IList<Mail> mails = null;

            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/OneSubscriberOneComicTwiceAsFast_BuildsOneMailWithBothEpisodes.xml")))
            {
                var now = DateTime.Now;
                var target = new ComicMailBuilder(
                    now,
                    new SimpleConfigurationParser($"blair.conrad@gmail.com: breaking-cat-news*2-20170327-{now.ToString("yyyyMMdd")}"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].From.Address.Should().Be("comics@blairconrad.com");
            mails[0].Personalization[0].Tos.Should().HaveCount(1);
            mails[0].Personalization[0].Tos[0].Address.Should().Be("blair.conrad@gmail.com");
            mails[0].Contents[0].Value.Should().Contain(BreakingCatNews20170327ImageUrl);
            mails[0].Contents[0].Value.Should().Contain("alt='breaking-cat-news on 27 March 2017'");
            mails[0].Contents[0].Value.Should().Contain(BreakingCatNews20170328ImageUrl);
            mails[0].Contents[0].Value.Should().Contain("alt='breaking-cat-news on 28 March 2017'");
        }

        [Fact]
        public void OneSubscriberOneComicFiveTimesAsFastOnlyThreeNewComicsLeft_OnlyRequestsThreeComics()
        {
            var today = new DateTime(2019, 2, 19);

            var fakeComicFetcher = A.Fake<IComicFetcher>();
            var target = new ComicMailBuilder(
                today,
                new SimpleConfigurationParser($"blair.conrad@gmail.com: breaking-cat-news*5-20190217-20190219"),
                fakeComicFetcher,
                A.Dummy<ILogger>());

            target.CreateMailMessage().ToList();

            A.CallTo(() => fakeComicFetcher.GetContent(A<string>._)).MustHaveHappened(3, Times.Exactly);
        }

        [Fact]
        public void TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic()
        {
            IList<Mail> mails = null;

            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic.xml")))
            {
                var target = new ComicMailBuilder(
                    DateTime.Now,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: 9chickweedlane; anyone@mail.org: dilbert"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(2);

            mails[0].From.Address.Should().Be("comics@blairconrad.com");
            mails[0].Personalization[0].Tos.Should().HaveCount(1);
            mails[0].Personalization[0].Tos[0].Address.Should().Be("blair.conrad@gmail.com");
            mails[0].Contents[0].Value.Should().Contain(ChickweedLaneUrl);

            mails[1].From.Address.Should().Be("comics@blairconrad.com");
            mails[1].Contents[0].Value.Should().Contain(DilbertImageUrl);
            mails[1].Personalization[0].Tos.Should().HaveCount(1);
            mails[1].Personalization[0].Tos[0].Address.Should().Be("anyone@mail.org");
        }

        [Fact]
        public void SubscribesToComicsKingdomComics_BuildsOneMailWithBothComics()
        {
            IList<Mail> mails = null;

            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/SubscribesToComicsKingdomComics_BuildsOneMailWithBothComics.xml")))
            {
                var target = new ComicMailBuilder(
                    new DateTime(2019, 7, 14), 
                    new SimpleConfigurationParser("blair.conrad@gmail.com: blondie, rhymes-with-orange"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should()
                .Contain(BlondieUrl, "it should have Blondie").And
                .Contain(RhymesWithOrangeUrl, "it should have Rhymes with Orange");
        }

        [Theory]
        [InlineData("dilbert", "http://www.dilbert.com/")]
        [InlineData("blondie", "https://www.comicskingdom.com/blondie/2018-06-27/")]
        [InlineData("9chickweedlane", "http://www.gocomics.com/9chickweedlane/2018/06/27/")]
        public void SubscribesToOneComic_QueriesFetcherWithCorrectUrl(string comic, string expectedUrl)
        {
            var fakeComicFetcher = A.Fake<IComicFetcher>();

            var target = new ComicMailBuilder(
                new DateTime(2018, 6, 27),
                new SimpleConfigurationParser($"blair.conrad@gmail.com: {comic}"),
                fakeComicFetcher,
                A.Dummy<ILogger>());

            target.CreateMailMessage().ToList();

            A.CallTo(() => fakeComicFetcher.GetContent(expectedUrl)).MustHaveHappened();
        }

        [Theory]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public void DinosaurComicOnAWeekend_MailIndicatesComicNotPublishedToday(DayOfWeek dayOfWeek)
        {
            IList<Mail> mails = null;

            var dateToCheck = MostRecent(dayOfWeek);
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/DinosaurComicsOn" + dayOfWeek + ".xml")))
            {
                var target = new ComicMailBuilder(
                    dateToCheck,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: dinosaur-comics"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should()
                .NotContain("Couldn't find comic for dinosaur-comics", "it should not have looked for the comic").And
                .Contain($"No published comic for dinosaur-comics on {dateToCheck.ToString("dd MMMM yyyy")}.", "it should tell the reader why there's no comic");
        }

        [Theory]
        [InlineData(DayOfWeek.Monday)]
        [InlineData(DayOfWeek.Tuesday)]
        [InlineData(DayOfWeek.Wednesday)]
        [InlineData(DayOfWeek.Thursday)]
        [InlineData(DayOfWeek.Friday)]
        public void DinosaurComicOnAWeekday_MailIncludesComic(DayOfWeek dayOfWeek)
        {
            IList<Mail> mails = null;

            var dateToCheck = MostRecent(dayOfWeek);
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/DinosaurComicsOn" + dayOfWeek + ".xml")))
            {
                var target = new ComicMailBuilder(
                    dateToCheck,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: dinosaur-comics"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should()
                .NotContain("Couldn't find comic for dinosaur-comics", "it should not have looked for the comic").And
                .NotContain($"Comic dinosaur-comics wasn't published on {dateToCheck.ToString("dd MMMM yyyy")}.", "it should have found the comic");
        }

        [Theory]
        [InlineData(DayOfWeek.Monday)]
        [InlineData(DayOfWeek.Tuesday)]
        [InlineData(DayOfWeek.Wednesday)]
        [InlineData(DayOfWeek.Thursday)]
        [InlineData(DayOfWeek.Friday)]
        [InlineData(DayOfWeek.Saturday)]
        public void FoxtrotOnAnythingButSunday_MailIndicatesComicNotPublishedToday(DayOfWeek dayOfWeek)
        {
            IList<Mail> mails = null;

            var dateToCheck = MostRecent(dayOfWeek);
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/FoxTrotOn" + dayOfWeek + ".xml")))
            {
                var target = new ComicMailBuilder(
                    dateToCheck,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: foxtrot"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should()
                .NotContain("Couldn't find comic for foxtrot", "it should not have looked for the comic").And
                .Contain($"No published comic for foxtrot on {dateToCheck.ToString("dd MMMM yyyy")}.", "it should tell the reader why there's no comic");
        }

        [Fact]
        public void FoxtrotOnSunday_MailIncludesComic()
        {
            IList<Mail> mails = null;

            var dateToCheck = MostRecent(DayOfWeek.Sunday);
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/FoxTrotOnSunday.xml")))
            {
                var target = new ComicMailBuilder(
                    dateToCheck,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: foxtrot"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should()
                .NotContain("Couldn't find comic for foxtrot", "it should not have looked for the comic").And
                .NotContain($"Comic foxtrot wasn't published on {dateToCheck.ToString("dd MMMM yyyy")}.", "it should have found the comic");
        }

        [Fact]
        public void CalvinAndHobbesOnSunday_MailIncludesComic()
        {
            IList<Mail> mails = null;

            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../../RecordedCalls/CalvinAndHobbesOnSunday.xml")))
            {
                var dateToCheck = MostRecent(DayOfWeek.Sunday);
                var target = new ComicMailBuilder(
                    dateToCheck,
                    new SimpleConfigurationParser("blair.conrad@gmail.com: calvinandhobbes"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                mails = target.CreateMailMessage().ToList();
            }

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should()
                .NotContain("Couldn't find comic for calvinandhobbes", "it should not have looked for the comic").And
                .Contain(CalvinAndHobbesSundayUrl, "it should have found the comic");
        }

        [Fact]
        public void TwoComicsOneThrowsWhenFetched_BuildsOneMailWithOneComicOneError()
        {
            IList<Mail> mails = null;

            var fakeComicFetcher = A.Fake<IComicFetcher>();
            A.CallTo(() => fakeComicFetcher.GetContent("http://rhymeswithorange.com/"))
                .Throws(new WebException("Bad Request"));
            A.CallTo(() => fakeComicFetcher.GetContent("http://www.dilbert.com/"))
                .Returns(@"<img class=""img-responsive img-comic"" src=""//assets.amuniversal.com/cfa39b00b39601365f19005056a9545d"" />");

            var target = new ComicMailBuilder(
                DateTime.Now,
                new SimpleConfigurationParser("blair.conrad@gmail.com: rhymeswithorange, dilbert"),
                fakeComicFetcher,
                A.Dummy<ILogger>());

            mails = target.CreateMailMessage().ToList();

            mails.Should().HaveCount(1);

            mails[0].Contents[0].Value.Should().Contain(DilbertImageUrl);
            mails[0].Contents[0].Value.Should().Contain("Couldn't find comic for rhymeswithorange");
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
