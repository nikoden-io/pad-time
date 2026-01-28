namespace PadTime.API.Authorization;

public static class Policies
{
    public const string RequireUser = "RequireUser";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireGlobalAdmin = "RequireGlobalAdmin";
    public const string RequireSiteAdmin = "RequireSiteAdmin";
}
