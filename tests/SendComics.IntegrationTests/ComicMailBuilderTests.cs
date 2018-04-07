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

        [Fact]
        public void OneSubscriberTwoComics_BuildsOneMailWithBothComics()
        {
            using (var fakeComicFetcher = SelfInitializingFake.For<IComicFetcher>(
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
                mails[0].Subject.Should().Be("comics 08 April 2018");
                mails[0].Personalization[0].Tos.Should().HaveCount(1);
                mails[0].Personalization[0].Tos[0].Address.Should().Be("blair.conrad@gmail.com");
                mails[0].Contents[0].Value.Should().Contain(DilbertImageUrl);
                mails[0].Contents[0].Value.Should().Contain(ChickweedLaneUrl);
            }
        }

        [Fact]
        public void TwoSubscribersOneComicEach_BuildsTwoMailsEachWithOneComic()
        {
            using (var fakeComicFetcher = SelfInitializingFake.For<IComicFetcher>(
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
    }
}