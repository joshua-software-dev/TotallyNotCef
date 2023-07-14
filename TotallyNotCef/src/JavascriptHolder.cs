namespace TotallyNotCef;

public class JavascriptHolder
{
    public const string DisableInjectionScript =
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
        console.log("fake WebSocket proxy");
        """;

    public const string EnableInjectionScript =
        """
        window.__WebSocket = window.WebSocket;
        window.RegisteredWebSockets = []
        const WebSocketProxy = new Proxy
        (
            window.WebSocket,
            {
                construct: function(target, args)
                {
                    const instance = new target(...args);
                    instance.registeredFunctions = {};

                    const addEventListenerProxy = new Proxy
                    (
                        instance.addEventListener,
                        {
                            apply: function(target, thisArg, args)
                            {
                                instance.registeredFunctions[args[0]] = args[1];
                                target.apply(thisArg, args);
                            }
                        }
                    );
                    instance.addEventListener = addEventListenerProxy;

                    window.RegisteredWebSockets.push(instance);
                    return instance;
                }
            }
        );

        window.WebSocket = WebSocketProxy;
        console.log("real WebSocket proxy");
        """;
}
