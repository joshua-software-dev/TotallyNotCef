namespace TotallyNotCef;

public class JavascriptHolder
{
    public const string InjectionScript =
        """
        window.__WebSocket = window.WebSocket;
        window.RegisteredWebSockets = []
        const WebSocketProxy = new Proxy
        (
            window.WebSocket,
            {
                construct: function(target, args)
                {
                const FakeWebSocket = function ()
                    {
                        this.index = window.RegisteredWebSockets.length;
                        this.registeredFunctions = {};
                        this.addEventListener = (name, f) =>
                        {
                            this.registeredFunctions[name] = f;
                        };
                    };

                const instance = new FakeWebSocket();
                window.RegisteredWebSockets.push(instance);

                return instance;
                }
            }
        );

        window.WebSocket = WebSocketProxy;
        console.log("fake WebSocket");
        """;
}
