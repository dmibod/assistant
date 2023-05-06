namespace Common.Core.Utils;

public static class TypeExtensions
{
    public static bool IsGenericOf(this Type type, Type genericInterface)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
    }
    
    public static IEnumerable<Type> GenericParametersOf(this Type type, Type genericInterface)
    {
        return type.IsGenericOf(genericInterface) 
            ? type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface).GetGenericArguments() 
            : Array.Empty<Type>();
    }

    public static Type GenericParameterOf(this Type type, Type genericInterface)
    {
        return type.GenericParametersOf(genericInterface).First();
    }

    public static Type GenericTypeFrom(this Type type, Type genericInterface)
    {
        return genericInterface.MakeGenericType(type);
    }
    
    public static bool HasTypeAttribute<T>(this Type type) where T : Attribute
    {
        return Attribute.GetCustomAttribute(type, typeof(T)) != null;
    }

    public static T GetTypeAttribute<T>(this Type type) where T : Attribute
    {
        return (T) Attribute.GetCustomAttribute(type, typeof(T))!;
    }
}