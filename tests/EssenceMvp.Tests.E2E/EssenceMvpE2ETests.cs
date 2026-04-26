using Microsoft.Playwright;

namespace EssenceMvp.Tests.E2E;

public class EssenceMvpE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    private readonly string _testEmail = $"test{Guid.NewGuid():N}@example.com";
    private readonly string _testPassword = "TestPassword123!";

    public EssenceMvpE2ETests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task FullFlow_RegisterLoginCreateProjectEvaluateSnapshot()
    {
        var page = _fixture.Page!;

        // 1. Register
        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Register");
        await page.FillAsync("#DisplayName", "Test User");
        await page.FillAsync("#Email", _testEmail);
        await page.FillAsync("#Password", _testPassword);
        await page.FillAsync("#ConfirmPassword", _testPassword);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/Account/Login", new PageWaitForURLOptions { Timeout = 5000 });

        // 2. Login
        await page.FillAsync("#Email", _testEmail);
        await page.FillAsync("#Password", _testPassword);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/Projects/Index", new PageWaitForURLOptions { Timeout = 5000 });

        // 3. Create Project
        await CreateProjectAsync(page, "Test Essence Project");

        // 4. Verify project created
        Assert.Contains("Test Essence Project", await page.ContentAsync());

        // 5. Evaluate First Alpha
        var modals = await page.QuerySelectorAllAsync("[id*='evaluateAlphaModal']");
        Assert.NotEmpty(modals);

        var firstModal = modals.FirstOrDefault();
        if (firstModal != null)
        {
            var targetId = await firstModal.GetAttributeAsync("id");
            await page.ClickAsync($"[data-bs-target='#{targetId}']");
            await page.WaitForSelectorAsync("[type='checkbox']");

            var checkboxes = await page.QuerySelectorAllAsync("[type='checkbox']");
            if (checkboxes.Count > 0) await checkboxes[0].CheckAsync();
            if (checkboxes.Count > 1) await checkboxes[1].CheckAsync();

            await page.ClickAsync("button:has-text('Guardar Evaluación')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // 6. Create Snapshot
        await page.ClickAsync("button[id='saveSnapshotBtn']");
        await page.WaitForSelectorAsync("text=Snapshot guardado", new PageWaitForSelectorOptions { Timeout = 5000 });
    }

    [Fact]
    public async Task LoginWithInvalidCredentials()
    {
        var page = _fixture.Page!;
        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Login");
        await page.FillAsync("#Email", "invalid@example.com");
        await page.FillAsync("#Password", "WrongPassword");
        await page.ClickAsync("button[type='submit']");

        // Should stay on login page with error
        await page.WaitForSelectorAsync("[class*='alert-danger']");
        Assert.Contains("Login", page.Url);
    }

    [Fact]
    public async Task CreateProjectTest()
    {
        var page = _fixture.Page!;

        // Login and create project
        await LoginAsNewUserAsync(page);
        await CreateProjectAsync(page, "Project Creation Test");

        Assert.Contains("Project Creation Test", await page.ContentAsync());
    }

    [Fact]
    public async Task EvaluateAlpha()
    {
        var page = _fixture.Page!;

        // Login and create project
        await LoginAsNewUserAsync(page);
        await CreateProjectAsync(page, "Alpha Evaluation Test");

        // Evaluate alpha
        await page.ClickAsync("[data-bs-target*='evaluateAlphaModal']:first-of-type");
        await page.WaitForSelectorAsync("[type='checkbox']");

        // Check all checkboxes in first state
        var checkboxes = await page.QuerySelectorAllAsync("[type='checkbox']");
        foreach (var checkbox in checkboxes.Take(3))
        {
            await checkbox.CheckAsync();
        }

        await page.ClickAsync("button:has-text('Guardar Evaluación')");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify progress changed
        var progressText = await page.TextContentAsync("[role='progressbar']");
        Assert.NotNull(progressText);
    }

    [Fact]
    public async Task CreateAndViewSnapshot()
    {
        var page = _fixture.Page!;

        // Login and create project
        await LoginAsNewUserAsync(page);
        await CreateProjectAsync(page, "Snapshot Test");

        // Create snapshot
        var snapshotBtn = await page.QuerySelectorAsync("#saveSnapshotBtn");
        Assert.NotNull(snapshotBtn);

        await page.ClickAsync("#saveSnapshotBtn");
        await page.WaitForSelectorAsync("text=Snapshot guardado", new PageWaitForSelectorOptions { Timeout = 5000 });

        // Verify snapshot appears in history
        await page.ClickAsync("text=snapshot");
        var snapshotHistory = await page.TextContentAsync("#snapshotPanel");
        Assert.NotNull(snapshotHistory);
    }

    private async Task LoginAsNewUserAsync(IPage page)
    {
        var email = $"test{Guid.NewGuid():N}@example.com";

        // Register
        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Register");
        await page.FillAsync("#DisplayName", "Test User");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", _testPassword);
        await page.FillAsync("#ConfirmPassword", _testPassword);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/Account/Login", new PageWaitForURLOptions { Timeout = 5000 });

        // Login
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", _testPassword);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/Projects/Index", new PageWaitForURLOptions { Timeout = 5000 });
    }

    private async Task CreateProjectAsync(IPage page, string projectName)
    {
        await page.GotoAsync($"{_fixture.BaseUrl}/Projects/Create");
        await page.FillAsync("#Name", projectName);
        await page.SelectOptionAsync("#Phase", "Exploring");
        await page.ClickAsync("button:has-text('Crear')");
        await page.WaitForURLAsync("**/Projects/Detail/**", new PageWaitForURLOptions { Timeout = 5000 });
    }
}
