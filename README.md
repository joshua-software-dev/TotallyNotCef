# TotallyNotCef
A simple CefSharp wrapper to provide html source from a CEF browser subprocess over HTTP.

## Configuration
If your environment does not allow you to download or run Chrome (as
TotallyNotCef will attempt to do on Linux), you can provide an environment
variable `CHROME_PATH` indicating the path at which Chrome should be found.
(For example, `/usr/local/bin/chromium`.)
