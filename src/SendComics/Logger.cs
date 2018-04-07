namespace SendComics
{
    using System;
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
            tracer.Info(GetTimestamp() + ' ' + message);
        }

        private static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }
    }
}
