namespace PadTime.Tests.TestSupport;

internal static class EntityTestExtensions
{
    public static void SetEntityId<T>(this T entity, Guid id)
        where T : class
    {
        var currentType = typeof(T);

        while (currentType is not null)
        {
            var property = currentType.GetProperty("Id");
            if (property is not null)
            {
                property.SetValue(entity, id);
                return;
            }

            currentType = currentType.BaseType;
        }

        throw new InvalidOperationException("Entity Id property was not found.");
    }
}
