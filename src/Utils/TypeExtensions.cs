
public static class TypeExtensions
{
    public static void SetPropertyValue(
    this object obj,
    string propertyName,
    object value
) => obj.GetType().GetProperty(propertyName)?.SetValue(obj, value, null);

    public static T? GetPropertyValue<T>(
        this object obj,
        string propName
    )
    {

        var val = obj.GetType().GetProperty(propName)?.GetValue(obj, null);
        if (val != null)
            return (T)val;
        return default(T);
    }


}