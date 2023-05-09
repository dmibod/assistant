namespace Common.Core.Utils;

public static class RenderUtils
{
    public static Tuple<string, string> Green = new("color", "green");
    public static Tuple<string, string> Red = new("color", "red");
    
    public static IDictionary<string, string> GreenStyle => CreateStyle(Green);

    public static IDictionary<string, string> RedStyle => CreateStyle(Red);

    public static string PairToContent(string name, string value)
    {
        return "{" + $"key:{name}," + $"value:{value}" + "}";
    }

    public static string PropToContent(string prop, IDictionary<string, string>? style = null)
    {
        var styleContent = StyleToContent(style);
        styleContent = string.IsNullOrEmpty(styleContent) ? styleContent : ", style:" + styleContent;
        return "{" + $"text:'{prop}'" + styleContent + "}";
    }

    public static string StyleToContent(IDictionary<string, string>? style)
    {
        return style == null || style.Count == 0
            ? string.Empty
            : "{" + style.Select(p => p.Key + ":'" + p.Value + "'").Aggregate((curr, i) => $"{curr},{i}") + "}";
    }

    public static Dictionary<string, string> CreateStyle(params Tuple<string, string>[] tuples)
    {
        return tuples.ToDictionary(t => t.Item1, t => t.Item2);
    }
}