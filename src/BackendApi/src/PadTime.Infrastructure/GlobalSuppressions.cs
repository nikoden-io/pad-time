using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1861:Prefer 'static readonly' fields over constant array arguments",
    Justification = "entity framework migrations are generated code",
    Scope = "namespaceanddescendants",
    Target = "PadTime.Infrastructure.Migrations")]
