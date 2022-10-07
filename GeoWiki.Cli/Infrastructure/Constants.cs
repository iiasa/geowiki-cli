namespace GeoWiki.Cli.Infrastructure
{
    public static class Constants
    {

#if DEBUG
        public const string ApiUrl = "https://localhost:44390";
        public const string IdentityUrl = "https://localhost:44386";
        public const bool IgnoreSslErrors = true;
#else
        public const string ApiUrl = "https://api.v2.geo-wiki.org";
        public const string IdentityUrl = "https://id.v2.geo-wiki.org";
        public const bool IgnoreSslErrors = false;
#endif
    }
}