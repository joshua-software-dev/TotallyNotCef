using CefSharp;
using CefSharp.OffScreen;
using NetCoreServer;
using System;
using System.Net;
using System.Threading;

namespace TotallyNotCef;

public class BrowserHtmlSourceHttpServer : NetCoreServer.HttpServer
{
    private class SimpleHttpSession : HttpSession
    {
        private readonly ChromiumWebBrowser _browser;
        private readonly CancellationTokenSource _cts;

        public SimpleHttpSession
        (
            NetCoreServer.HttpServer server,
            ChromiumWebBrowser browser,
            CancellationTokenSource cts
        ) : base(server)
        {
            _browser = browser;
            _cts = cts;
        }

        protected override void OnReceivedRequest(HttpRequest request)
        {
            switch (request.Method)
            {
                case "GET":
                {
                    Console.WriteLine($"GET  {DateTime.Now} | {request.Url}");

                    if (request.Url == "/shutdown")
                    {
                        SendResponseAsync(Response.MakeGetResponse("Goodbye"));
                        _cts.Cancel();
                        _cts.Dispose();
                        System.Environment.Exit(0);
                    }

                    var source = _browser.GetSourceAsync().GetAwaiter().GetResult() ?? string.Empty;
                    SendResponseAsync(Response.MakeGetResponse(source));
                    break;
                }
                case "HEAD":
                {
                    Console.WriteLine($"HEAD {DateTime.Now} | {request.Url}");
                    SendResponseAsync(Response.MakeHeadResponse());
                    break;
                }
                default:
                {
                    SendResponseAsync(Response.MakeErrorResponse("Unsupported HTTP method: " + request.Method));
                    break;
                }
            }
        }
    }

    private readonly ChromiumWebBrowser _browser;
    private readonly CancellationTokenSource _cts;

    public BrowserHtmlSourceHttpServer
    (
        IPAddress address,
        int port,
        ChromiumWebBrowser browser,
        CancellationTokenSource cts
    ) : base (address, port)
    {
        _browser = browser;
        _cts = cts;
    }

    protected override TcpSession CreateSession()
    {
        return new SimpleHttpSession(this, _browser, _cts);
    }
}
