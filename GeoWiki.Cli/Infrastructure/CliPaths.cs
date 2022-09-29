namespace GeoWiki.Cli.Infrastructure
{
    public static class CliPaths
    {
        public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".geowiki");
    }
}
