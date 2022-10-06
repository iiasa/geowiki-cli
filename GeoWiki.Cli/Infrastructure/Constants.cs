namespace GeoWiki.Cli.Infrastructure
{
    public static class Constants
    {
        public const string ApiUrl = "https://localhost:44390";
        public const string IdentityUrl = "https://localhost:44386";
#if DEBUG
        public const bool IgnoreSslErrors = true;
#else
        public const bool IgnoreSslErrors = false;
#endif
    }
}