namespace SendComics.Services;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Playwright;

internal sealed class WebComicFetcher : IComicFetcher
{
    public string GetContent(Uri url, Func<string, bool> isReady)
    {
        return GetContentWithBrowser(url, isReady).Result;
    }

    private static async Task<string> GetContentWithBrowser(Uri url, Func<string, bool> isReady)
    {
        var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
#pragma warning disable CA2007
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false }).ConfigureAwait(false);
        await using var context = await browser.NewContextAsync().ConfigureAwait(false);
#pragma warning restore CA2007
        var page = await context.NewPageAsync().ConfigureAwait(false);

        await page.GotoAsync(url.AbsoluteUri, new() { WaitUntil = WaitUntilState.DOMContentLoaded }).ConfigureAwait(false);
        await page.WaitForFunctionAsync("document.title !== 'Establishing a secure connection ...'").ConfigureAwait(false);
        var timeout = TimeSpan.FromSeconds(30);
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            var content = await page.ContentAsync().ConfigureAwait(false);
            if (isReady(content))
            {
                return content;
            }

            if (stopwatch.Elapsed >= timeout)
            {
                throw new TimeoutException($"Comic content was not ready within {timeout.TotalSeconds:0} seconds.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250)).ConfigureAwait(false);
        }
    }
}
