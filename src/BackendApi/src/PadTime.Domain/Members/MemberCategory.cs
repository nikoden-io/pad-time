namespace PadTime.Domain.Members;

/// <summary>
/// Member category determines booking window and site restrictions.
/// </summary>
public enum MemberCategory
{
    /// <summary>
    /// Global member (Gxxxx) - Can book J-21, all sites.
    /// </summary>
    Global = 1,

    /// <summary>
    /// Site member (Sxxxxx) - Can book J-14, restricted to assigned site.
    /// </summary>
    Site = 2,

    /// <summary>
    /// Free member (Lxxxxx) - Can book J-5, all sites.
    /// </summary>
    Free = 3
}
