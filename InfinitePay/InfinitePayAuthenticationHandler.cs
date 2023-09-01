namespace InfinitePay;

public class InfinitePayAuthenticationHandler : DelegatingHandler
{
    public InfinitePayAuthenticationHandler()
    {
        InnerHandler = new HttpClientHandler();
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("authorization", $"Bearer {Auth.Token}");
        var response = await base.SendAsync(request, CancellationToken.None);
        var b = await response.Content.ReadAsStringAsync(cancellationToken);
        return response;
    }
}