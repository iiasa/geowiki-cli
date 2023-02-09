using GeoWiki.Cli.Infrastructure;
using IdentityModel.OidcClient;
using Spectre.Console;

namespace GeoWiki.Cli.Services;

public class AuthService
{
    static OidcClient? _oidcClient;
    public AuthService()
    {

    }

    public async Task<string> GetAccessTokenAsync()
    {
        var accessToken = await File.ReadAllTextAsync(CliPaths.AccessToken);
        return accessToken;
    }

    public async Task<string> GetTenantAsync()
    {
        try
        {
            var tenant = await File.ReadAllTextAsync(CliPaths.Tenant);
            return tenant;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return string.Empty;
        }
    }

    public async Task<string> SwitchTenantAsync(string tenant)
    {
        await File.WriteAllTextAsync(CliPaths.Tenant, tenant);
        return tenant;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (!File.Exists(CliPaths.AccessToken))
            return false;

        var accessToken = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(accessToken);
    }

    public async Task LoginAsync()
    {
        // create a redirect URI using an available port on the loopback address.
        // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
        var browser = new SystemBrowser(3000);
        string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

        var options = new OidcClientOptions
        {
            Authority = Constants.IdentityUrl,
            ClientId = "GeoWiki_React_NextJs_Public",
            RedirectUri = redirectUri,
            Scope = "openid profile roles GeoWiki",
            FilterClaims = false,
            Browser = browser,
        };

        _oidcClient = new OidcClient(options);
        var result = await _oidcClient.LoginAsync(new LoginRequest());
        if(result.IsError)
        {
            Console.WriteLine(result.Error);
            return;
        }
        if(!Directory.Exists(CliPaths.Root))
        {
            Directory.CreateDirectory(CliPaths.Root);
        }

        if(!Directory.Exists(CliPaths.Root))
        {
            Directory.CreateDirectory(CliPaths.Root);
        }
        Console.WriteLine("Login successful");
        await File.WriteAllTextAsync(CliPaths.AccessToken, result.AccessToken);
        await File.WriteAllTextAsync(CliPaths.RefreshToken, result.RefreshToken);
    }

    // private static async Task NextSteps(LoginResult result)
    // {
    //     var currentAccessToken = result.AccessToken;
    //     var currentRefreshToken = result.RefreshToken;

    //     var menu = "  x...exit  c...call api   ";
    //     if (currentRefreshToken != null) menu += "r...refresh token   ";

    //     while (true)
    //     {
    //         Console.WriteLine("\n\n");

    //         Console.Write(menu);
    //         var key = Console.ReadKey();

    //         if (key.Key == ConsoleKey.X) return;
    //         // if (key.Key == ConsoleKey.C) await CallApi(currentAccessToken);
    //         if (key.Key == ConsoleKey.R)
    //         {
    //             var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);
    //             if (refreshResult.IsError)
    //             {
    //                 Console.WriteLine($"Error: {refreshResult.Error}");
    //             }
    //             else
    //             {
    //                 currentRefreshToken = refreshResult.RefreshToken;
    //                 currentAccessToken = refreshResult.AccessToken;

    //                 Console.WriteLine("\n\n");
    //                 Console.WriteLine($"access token:   {refreshResult.AccessToken}");
    //                 Console.WriteLine($"refresh token:  {refreshResult?.RefreshToken ?? "none"}");
    //             }
    //         }
    //     }
    // }

    // private static void ShowResult(LoginResult result)
    // {
    //     if (result.IsError)
    //     {
    //         Console.WriteLine("\n\nError:\n{0}", result.Error);
    //         return;
    //     }

    //     Console.WriteLine("\n\nClaims:");
    //     foreach (var claim in result.User.Claims)
    //     {
    //         Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
    //     }

    //     Console.WriteLine($"\nidentity token: {result.IdentityToken}");
    //     Console.WriteLine($"access token:   {result.AccessToken}");
    //     Console.WriteLine($"refresh token:  {result?.RefreshToken ?? "none"}");
    // }

    // private static async Task CallApi(string currentAccessToken)
    // {
    //     _apiClient.SetBearerToken(currentAccessToken);
    //     var response = await _apiClient.GetAsync("");

    //     if (response.IsSuccessStatusCode)
    //     {
    //         var json = JArray.Parse(await response.Content.ReadAsStringAsync());
    //         Console.WriteLine("\n\n");
    //         Console.WriteLine(json);
    //     }
    //     else
    //     {
    //         Console.WriteLine($"Error: {response.ReasonPhrase}");
    //     }
    // }

    public void Logout()
    {
        if (File.Exists(CliPaths.AccessToken))
            File.Delete(CliPaths.AccessToken);

        if (File.Exists(CliPaths.RefreshToken))
            File.Delete(CliPaths.RefreshToken);

        if (File.Exists(CliPaths.Tenant))
            File.Delete(CliPaths.Tenant);
    }
}