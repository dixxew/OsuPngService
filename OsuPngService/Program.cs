using PuppeteerSharp;

internal class Program
{
    static IBrowser? browser = null;
    static readonly SemaphoreSlim browserLock = new(1, 1);


    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/osupng", async () =>
        {
            var pngBytes = await GenerateOsuImageAsync();
            return Results.File(pngBytes, "image/png");
        });

        app.Run();
    }

    static async Task<IBrowser> GetBrowserAsync()
    {
        if (browser != null) return browser;

        await browserLock.WaitAsync();
        try
        {
            if (browser != null) return browser;

            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync();

            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu"
                }
            });

            return browser;
        }
        finally
        {
            browserLock.Release();
        }
    }

    static async Task<byte[]> GenerateOsuImageAsync()
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates/osupng", "template.html");
        var template = await File.ReadAllTextAsync(templatePath);
        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Templates", "osupng");

        // Конвертим лого в base64
        var mmLogo = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(assetsPath, "mm_logo.png")));
        var tgLogo = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(assetsPath, "Telegram_2019_simple_logo.svg.png")));
        var vkLogo = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(assetsPath, "VK Logo.png")));
        var ytLogo = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(assetsPath, "yt_icon_red_digital.png")));
        var osuLogo = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(assetsPath, "osu-logo.png")));
        var ghLogo = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(assetsPath, "GitHub_Invertocat_Black.png")));

        var data = new Dictionary<string, string>
        {
            { "mm_logo", $"data:image/png;base64,{mmLogo}" },
            { "tg_logo", $"data:image/png;base64,{tgLogo}" },
            { "vk_logo", $"data:image/png;base64,{vkLogo}" },
            { "yt_logo", $"data:image/png;base64,{ytLogo}" },
            { "osu_logo", $"data:image/png;base64,{osuLogo}" },
            { "gh_logo", $"data:image/png;base64,{ghLogo}" },
        };

        var html = template;
        foreach (var (key, value) in data)
        {
            html = html.Replace($"{{{{{key}}}}}", value);
        }

        // Рендерим
        var b = await GetBrowserAsync();
        await using var page = await b.NewPageAsync();

        await page.SetViewportAsync(new ViewPortOptions
        {
            Width = 1000,
            Height = 700,
            DeviceScaleFactor = 2
        });

        await page.SetContentAsync(html);
        await page.WaitForFunctionAsync("() => document.fonts.ready");

        var screenshot = await page.ScreenshotDataAsync(new ScreenshotOptions
        {
            Type = ScreenshotType.Png,
            FullPage = false
        });

        return screenshot;
    }
}