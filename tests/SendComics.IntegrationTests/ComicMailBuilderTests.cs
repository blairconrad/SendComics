namespace SendComics.IntegrationTests
{
    using System.Linq;
    using FakeItEasy;
    using FluentAssertions;
    using global::SendComics.Services;
    using SelfInitializingFakes;
    using Xunit;

    public class ComicMailBuilderTests
    {
        private const string DilbertImageUrl = "http://assets.amuniversal.com/9c0154c0f27a01351756005056a9545d";
        private const string ChickweedLaneUrl = "http://assets.amuniversal.com/d52e0040ffde01351cbd005056a9545d";
        private const string BlondieUrl = "https://safr.kingfeatures.com/idn/cnfeed/zone/js/content.php?file=aHR0cDovL3NhZnIua2luZ2ZlYXR1cmVzLmNvbS9CbG9uZGllLzIwMTgvMDQvQmxvbmRpZS4yMDE4MDQxMF85MDAuZ2lm";
        private const string RhymesWithOrangeUrl = "https://safr.kingfeatures.com/idn/cnfeed/zone/js/content.php?file=aHR0cDovL3NhZnIua2luZ2ZlYXR1cmVzLmNvbS9SaHltZXNXaXRoT3JhbmdlLzIwMTgvMDQvUmh5bWVzX3dpdGhfT3JhbmdlLjIwMTgwNDEwXzkwMC5naWY=";

        [Fact]
        public void OneSubscriberTwoComics_BuildsOneMailWithBothComics()
        {
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../RecordedCalls/OneSubscriberTwoComics_BuildsOneMailWithBothComics.xml")))
            {
                var target = new ComicMailBuilder(
                    new SimpleConfigurationParser("blair.conrad@gmail.com: dilbert, 9chickweedlane"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                var mails = target.CreateMailMessage().ToList();

                mails.Should().HaveCount(1);

                mails[0].From.Address.Should().Be("comics@blairconrad.com");
                mails[0].Personalization[0].Tos.Should().HaveCount(1);
                mails[0].Personalization[0].Tos[0].Address.Should().Be("blair.conrad@gmail.com");
                mails[0].Contents[0].Value.Should().Contain(DilbertImageUrl);
                mails[0].Contents[0].Value.Should().Contain(ChickweedLaneUrl);
            }
        }

        [Fact]
        public void TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic()
        {
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../RecordedCalls/TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic.xml")))
            {
                var target = new ComicMailBuilder(
                    new SimpleConfigurationParser("blair.conrad@gmail.com: 9chickweedlane; anyone@mail.org: dilbert"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                var mails = target.CreateMailMessage().ToList();

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
        }

        [Fact]
        public void SubscribesToKingsFeatureComics_BuildsOneMailWithBothComics()
        {
            using (var fakeComicFetcher = SelfInitializingFake<IComicFetcher>.For(
                () => new WebComicFetcher(),
                new XmlFileRecordedCallRepository("../../RecordedCalls/SubscribesToKingsFeatureComics_BuildsOneMailWithBothComics.xml")))
            {
                var target = new ComicMailBuilder(
                    new SimpleConfigurationParser("blair.conrad@gmail.com: blondie, rhymeswithorange"),
                    fakeComicFetcher.Object,
                    A.Dummy<ILogger>());

                var mails = target.CreateMailMessage().ToList();

                mails.Should().HaveCount(1);

                mails[0].Contents[0].Value.Should()
                    .Contain(BlondieUrl, "it should have Blondie").And
                    .Contain(RhymesWithOrangeUrl, "it should have Rhymes with Orange");
            }
        }

	    [Theory]
		[InlineData("dilbert", "http://www.dilbert.com/")]
		[InlineData("blondie", "http://blondie.com/")]
		[InlineData("9chickweedlane", "http://www.gocomics.com/9chickweedlane/2018/06/27/")]
	    public void SubscribesToOneComic_QueriesFetcherWithCorrectUrl(string comic, string expectedUrl)
	    {
		    var fakeComicFetcher = A.Fake<IComicFetcher>();

		    var target = new ComicMailBuilder(
			    new SimpleConfigurationParser($"blair.conrad@gmail.com: {comic}"),
			    fakeComicFetcher,
			    A.Dummy<ILogger>());

		    target.CreateMailMessage().ToList();

		    A.CallTo(() => fakeComicFetcher.GetContent(expectedUrl)).MustHaveHappened();
	    }

	}
}