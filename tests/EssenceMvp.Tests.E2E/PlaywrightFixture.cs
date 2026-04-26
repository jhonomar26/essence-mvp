using Microsoft.Playwright;

namespace EssenceMvp.Tests.E2E;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    public IPage? Page { get; private set; }

    public string BaseUrl { get; } = Environment.GetEnvironmentVariable("MVC_BASE_URL") ?? "https://localhost:7089";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await _browser.NewContextAsync();
        Page = await context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Page != null) await Page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
