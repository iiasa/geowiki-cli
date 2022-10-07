using System.Net.Http.Headers;
using GeoWiki.Cli.Services;

namespace GeoWiki.Cli.Handlers;
public class HeaderHandler : DelegatingHandler
{
    private readonly AuthService _authService;
    public HeaderHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("Authorization"))
        {
            string? token = await _authService.GetAccessTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        if (!request.Headers.Contains("__tenant"))
        {
            string? tenant = await _authService.GetTenantAsync();
            if (!string.IsNullOrEmpty(tenant))
            {
                request.Headers.Add("__tenant", tenant);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}