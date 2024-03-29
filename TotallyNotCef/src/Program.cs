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

using System;


namespace TotallyNotCef;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine($"Provide the following arguments: <url> <httpServerPort> <enableAudio> <enableWebSockets>");
            return 1;
        }
        var url = args[0];
        ushort.TryParse(args[1], out var httpServerPort);
        ushort.TryParse(args[2], out var enableAudio);
        ushort.TryParse(args[3], out var enableWebSockets);
        var enableAudioParsed = enableAudio switch
        {
            0 => false,
            1 => true,
            _ => throw new ArgumentOutOfRangeException(nameof(enableAudio))
        };
        var enableWebSocketsParsed = enableWebSockets switch
        {
            0 => false,
            1 => true,
            _ => throw new ArgumentOutOfRangeException(nameof(enableWebSockets))
        };

        #if OS_IS_WINDOWS
        var wrapper = new CefSharpBrowserWrapper();
        #else
        var wrapper = new PuppeteerSharpBrowserWrapper();
        #endif

        AsyncContext.Run
        (
            async delegate
            {
                await wrapper.Start(url, httpServerPort, enableAudioParsed, enableWebSocketsParsed);
            }
        );

        return 0;
    }
}
