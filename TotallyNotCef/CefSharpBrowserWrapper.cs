// Parts of the following code are modified from from the CefSharp minimal
// example, which falls under the following LICENSE:

/*
The MIT License (MIT)

Copyright (c) 2013 The CefSharp Authors

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if OS_IS_WINDOWS || DEBUG
using CefSharp;
using CefSharp.OffScreen;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace TotallyNotCef;

public class CefSharpBrowserWrapper : ICefBrowserWrapper
{
    private ChromiumWebBrowser? _browser;

    public string? GetHtmlSource()
    {
        return _browser?.GetSourceAsync().GetAwaiter().GetResult();
    }

    public async Task Start(string url, ushort httpServerPort, bool enableAudio)
    {
        var settings = new CefSettings
        {
            //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
            LogSeverity = LogSeverity.Fatal
        };

        try
        {
            var cachePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Cache");
            if (!Path.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            settings.CachePath = cachePath;
        }
        catch (Exception) { }

        settings.DisableGpuAcceleration();
        if (enableAudio)
        {
            settings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";
            settings.CefCommandLineArgs.Remove("mute-audio");
        }

        Console.WriteLine(settings.CachePath);
        Console.WriteLine("Initializing CEF installation...");
        //Perform dependency check to make sure all relevant resources are in our output directory.
        if
        (
            !(
                await Cef.InitializeAsync
                (
                    settings,
                    performDependencyCheck: true,
                    browserProcessHandler: null
                )
            )
        )
        {
            throw new Exception("Unable to initialize CEF, check the log file.");
        }

        try
        {
            Console.WriteLine("CEF starting now...");
            // Create the CefSharp.OffScreen.ChromiumWebBrowser instance
            using (_browser = new ChromiumWebBrowser(url))
            {
                var initialLoadResponse = await _browser.WaitForInitialLoadAsync();
                if (!initialLoadResponse.Success)
                {
                    throw new Exception
                    (
                        string.Format
                        (
                            "Page load failed with ErrorCode:{0}, HttpStatusCode:{1}",
                            initialLoadResponse.ErrorCode,
                            initialLoadResponse.HttpStatusCode
                        )
                    );
                }

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
        }
        finally
        {
            // Clean up Chromium objects. You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }
    }
}
#endif
