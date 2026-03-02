namespace SendComics.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

/// <summary>
/// Manages a shared Playwright browser instance for efficient resource usage.
/// Lazily initializes on first use and shares the browser across all requests.
/// </summary>
internal sealed class PlaywrightBrowserService : IAsyncDisposable
{
    private readonly Lazy<Task<IBrowser>> lazyBrowser;
    private IPlaywright playwright;

    public PlaywrightBrowserService()
    {
        this.lazyBrowser = new Lazy<Task<IBrowser>>(this.InitializeBrowserAsync);
    }

    /// <summary>
    /// Gets a new page from the shared browser instance.
    /// Initializes Playwright and browser on first call.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page.</returns>
    public async Task<IPage> GetPageAsync()
    {
        var browser = await this.lazyBrowser.Value.ConfigureAwait(false);
        var context = await browser.NewContextAsync().ConfigureAwait(false);
        return await context.NewPageAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the Playwright browser and instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.lazyBrowser.IsValueCreated)
        {
            var browser = await this.lazyBrowser.Value.ConfigureAwait(false);
            await browser.DisposeAsync().ConfigureAwait(false);
        }

        this.playwright?.Dispose();
    }

    private async Task<IBrowser> InitializeBrowserAsync()
    {
        this.playwright = await Playwright.CreateAsync().ConfigureAwait(false);
        return await this.playwright.Chromium.LaunchAsync(new() { Headless = true }).ConfigureAwait(false);
    }
}
