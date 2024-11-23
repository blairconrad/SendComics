namespace SendComics;

using System;
using System.Globalization;

public class Logger : ILogger
{
    public void Info(string message)
    {
        Console.WriteLine("INFO  " + GetTimestamp() + ' ' + message);
    }

    public void Error(string message)
    {
        Console.WriteLine("ERROR " + GetTimestamp() + ' ' + message);
    }

    private static string GetTimestamp()
    {
        return DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    }
}
