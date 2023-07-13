namespace TotallyNotCef;

public interface ICefBrowserWrapper
{
    public string? GetHtmlSource();
    public void ForwardMessageToFakeWebSocket(string jsonString);
}
