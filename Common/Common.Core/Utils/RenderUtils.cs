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

    public static string PropsToContent(IEnumerable<string> props)
    {
        var content = props == null || props.Count() == 0
            ? string.Empty
            : props.Aggregate((curr, i) => $"{curr},{i}");
        
        return $"[{content}]";
    }

    public static string StyleToContent(IDictionary<string, string>? style)
    {
        return style == null || style.Count == 0
            ? string.Empty
            : "{" + style.Select(p => p.Key + ":'" + p.Value + "'").Aggregate((curr, i) => $"{curr},{i}") + "}";
    }

    public static IDictionary<string, string> CreateStyle(params Tuple<string, string>[] tuples)
    {
        return tuples.ToDictionary(t => t.Item1, t => t.Item2);
    }

    public static IDictionary<string, string> MergeStyle(params IDictionary<string, string>[] styles)
    {
        return styles
            .SelectMany(style => style)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}