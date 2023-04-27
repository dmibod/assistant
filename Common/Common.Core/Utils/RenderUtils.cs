namespace Common.Core.Utils;

public static class RenderUtils
{
    public static readonly IDictionary<string, string> GreenStyle = new Dictionary<string, string>
    {
        ["color"] = "green"
    };

    public static readonly IDictionary<string, string> RedStyle = new Dictionary<string, string>
    {
        ["color"] = "red"
    };
    
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