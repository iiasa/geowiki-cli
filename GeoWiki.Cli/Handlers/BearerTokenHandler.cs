using System.Net.Http.Headers;
using GeoWiki.Cli.Services;
namespace GeoWiki.Cli.Handlers;
public class BearerTokenHandler : DelegatingHandler
{
    private readonly AuthService _authService;
    public BearerTokenHandler(AuthService authService)
    {
        _authService = authService;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("Authorization"))
        {
            // Fetch your token here
            string? token = await _authService.GetAccessTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}