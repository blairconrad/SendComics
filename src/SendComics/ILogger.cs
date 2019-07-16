namespace SendComics
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = nameof(Error), Justification = "Very common on loggers. No chance of confusion.")]
    public interface ILogger
    {
        void Info(string message);

        void Error(string message);
    }
}
