using GeoWiki.Cli.Services;
namespace GeoWiki.Cli.Handlers;
public class TenantHeaderHandler : DelegatingHandler
{
    private readonly AuthService _authService;
    public TenantHeaderHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
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