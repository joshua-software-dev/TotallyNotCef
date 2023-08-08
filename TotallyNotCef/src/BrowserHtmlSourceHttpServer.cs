using NetCoreServer;
using System;
using System.Net;
using System.Threading;


namespace TotallyNotCef;

public class BrowserHtmlSourceHttpServer : NetCoreServer.HttpServer
{
    private class SimpleHttpSession : HttpSession
    {
        private readonly ICefBrowserWrapper _browser;
        private readonly CancellationTokenSource _cts;

        public SimpleHttpSession
        (
            NetCoreServer.HttpServer server,
            ICefBrowserWrapper browser,
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
                    else if (request.Url == "/tncef")
                    {
                        SendResponseAsync
                        (
                            Response.MakeGetResponse
                            (
                                """
                                {"IsTotallyNotCef":true}
                                """
                            )
                        );
                        break;
                    }

                    var source = _browser.GetHtmlSource() ?? string.Empty;
                    SendResponseAsync(Response.MakeGetResponse(source));
                    break;
                }
                case "HEAD":
                {
                    Console.WriteLine($"HEAD {DateTime.Now} | {request.Url}");
                    SendResponseAsync(Response.MakeHeadResponse());
                    break;
                }
                case "POST":
                {
                    Console.WriteLine($"POST {DateTime.Now} | {request.Url}");
                    _browser.ForwardMessageToFakeWebSocket(request.Body);
                    SendResponseAsync(Response.MakeGetResponse());
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

    private readonly ICefBrowserWrapper _browser;
    private readonly CancellationTokenSource _cts;

    public BrowserHtmlSourceHttpServer
    (
        IPAddress address,
        int port,
        ICefBrowserWrapper browser,
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
