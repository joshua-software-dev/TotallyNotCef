#if !OS_IS_WINDOWS || DEBUG
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;
#endif


namespace TotallyNotCef;

public class PuppeteerSharpBrowserWrapper : ICefBrowserWrapper
{
    #if OS_IS_WINDOWS
    public string? GetHtmlSource() => string.Empty;
    public void ForwardMessageToFakeWebSocket(string jsonString) { }
    #elif !OS_IS_WINDOWS || DEBUG
    private IPage? _page;
    public string? GetHtmlSource() =>
        _page?.GetContentAsync().GetAwaiter().GetResult();

    public void ForwardMessageToFakeWebSocket(string jsonString) =>
        _page?
            .EvaluateExpressionAsync($"window.RegisteredWebSockets[0].registeredFunctions.message(new MessageEvent('message', {{data: '{System.Web.HttpUtility.JavaScriptStringEncode(jsonString)}'}}))")
            .GetAwaiter()
            .GetResult();

    public async Task Start(string url, ushort httpServerPort, bool enableAudio, bool enableWebSockets)
    {
        var options = new LaunchOptions { Headless = true };
        var chromePath = Environment.GetEnvironmentVariable("CHROME_PATH");
        if (chromePath != null)
        {
            Console.WriteLine($"Using CHROME_PATH: {chromePath}");
            options.ExecutablePath = chromePath;
        }
        else
        {
            var downloadPath = Path.Join
            (
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "ChromeDownload"
            );

            using (var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = downloadPath }))
            {
                if (!Path.Exists(downloadPath))
                {
                    Console.WriteLine($"Downloading Chrome...");
                    var info = await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                    Console.WriteLine($"Chrome downloaded to: {info.ExecutablePath}");
                    options.ExecutablePath = info.ExecutablePath;
                }
                else
                {
                    options.ExecutablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);
                    Console.WriteLine($"Using existing Chrome install: {options.ExecutablePath}");
                }
            }
        }

        if (enableAudio)
        {
            Console.WriteLine("Enabling audio...");
            options.IgnoredDefaultArgs = new [] { "--mute-audio" };
        }


        var browser = await Puppeteer.LaunchAsync(options);

        _page = await browser.NewPageAsync();
        if (enableWebSockets)
        {
            Console.WriteLine("Injecting javascript to track browser WebSockets...");
            await _page.EvaluateExpressionOnNewDocumentAsync(JavascriptHolder.EnableInjectionScript);
        }
        else
        {
            Console.WriteLine("Injecting javascript to disable browser WebSockets...");
            await _page.EvaluateExpressionOnNewDocumentAsync(JavascriptHolder.DisableInjectionScript);
        }
        await _page.GoToAsync(url);

        Console.WriteLine("Starting http server...");
        var cts = new CancellationTokenSource();
        var server = new BrowserHtmlSourceHttpServer
        (
            IPAddress.Parse("127.0.0.1"),
            httpServerPort,
            this,
            cts
        );

        try
        {
            server.Start();
        }
        catch (SocketException)
        {
            Console.WriteLine($"Could not bind to socket: {httpServerPort}, exiting...");
            System.Environment.Exit(2);
        }

        Console.WriteLine($"Listening on http://127.0.0.1:{httpServerPort}/");

        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(10, cts.Token);
        }
    }
    #endif
}
