using System.Text.RegularExpressions;
using PadTime.Domain.Common;

namespace PadTime.Domain.Members;

/// <summary>
/// Value object representing a member's business identifier.
/// Format: Gxxxx (Global), Sxxxxx (Site), or Lxxxxx (Free)
/// </summary>
public sealed partial class Matricule : ValueObject
{
    public string Value { get; }
    public MemberCategory Category { get; }

    private Matricule(string value, MemberCategory category)
    {
        Value = value;
        Category = category;
    }

    // EF Core constructor - reconstructs Category from Value
    private Matricule(string value)
    {
        Value = value;
        Category = DeriveCategory(value);
    }

    private static MemberCategory DeriveCategory(string value)
    {
        return value[0] switch
        {
            'G' => MemberCategory.Global,
            'S' => MemberCategory.Site,
            'L' => MemberCategory.Free,
            _ => throw new InvalidOperationException($"Invalid matricule prefix: {value[0]}")
        };
    }

    public static Result<Matricule> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Member.InvalidMatricule;

        var normalized = value.Trim().ToUpperInvariant();

        if (GlobalPattern().IsMatch(normalized))
            return new Matricule(normalized, MemberCategory.Global);

        if (SitePattern().IsMatch(normalized))
            return new Matricule(normalized, MemberCategory.Site);

        if (FreePattern().IsMatch(normalized))
            return new Matricule(normalized, MemberCategory.Free);

        return DomainErrors.Member.InvalidMatricule;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^G\d{4}$", RegexOptions.Compiled)]
    private static partial Regex GlobalPattern();

    [GeneratedRegex(@"^S\d{5}$", RegexOptions.Compiled)]
    private static partial Regex SitePattern();

    [GeneratedRegex(@"^L\d{5}$", RegexOptions.Compiled)]
    private static partial Regex FreePattern();
}
