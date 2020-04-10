namespace SendComics
{
    using System;
    using System.Globalization;
    using Microsoft.Azure.WebJobs.Host;

    public class Logger : ILogger
    {
        private readonly TraceWriter tracer;

        public Logger(TraceWriter tracer)
        {
            this.tracer = tracer;
        }

        public void Info(string message)
        {
            this.tracer.Info(GetTimestamp() + ' ' + message);
        }

        public void Error(string message)
        {
            this.tracer.Error(GetTimestamp() + ' ' + message);
        }

        private static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
