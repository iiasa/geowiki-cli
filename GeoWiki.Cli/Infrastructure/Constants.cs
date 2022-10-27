namespace GeoWiki.Cli.Infrastructure
{
    public static class Constants
    {

        public const string ApiUrl = "https://framework.api.v2.geo-wiki.org";
        public const string IdentityUrl = "https://framework.id.v2.geo-wiki.org";
        public const bool IgnoreSslErrors = false;

// #if DEBUG
//         public const string ApiUrl = "https://localhost:44390";
//         public const string IdentityUrl = "https://localhost:44386";
//         public const bool IgnoreSslErrors = true;
// #else
//         public const string ApiUrl = "https://framework.api.v2.geo-wiki.org";
//         public const string IdentityUrl = "https://framework.id.v2.geo-wiki.org";
//         public const bool IgnoreSslErrors = false;
// #endif
    }
}