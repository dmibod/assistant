namespace Common.Core.Utils;

public static class RenderUtils
{
    public static string PairToContent(string name, string value)
    {
        return "{" + $"key:{name}," + $"value:{value}" + "}";
    }

    public static string PropToContent(string prop, Dictionary<string, string>? style = null)
    {
        var styleContent = StyleToContent(style);
        styleContent = string.IsNullOrEmpty(styleContent) ? styleContent : ", style:" + styleContent;
        return "{" + $"text:'{prop}'" + styleContent + "}";
    }

    public static string StyleToContent(Dictionary<string, string>? style)
    {
        return style == null || style.Count == 0
            ? string.Empty
            : "{" + style.Select(p => p.Key + ":'" + p.Value + "'").Aggregate((curr, i) => $"{curr},{i}") + "}";
    }

    public static Dictionary<string, string> GreenStyle()
    {
        var style = new Dictionary<string, string>();

        style.Add("color", "green");

        return style;
    }

    public static Dictionary<string, string> RedStyle()
    {
        var style = new Dictionary<string, string>();

        style.Add("color", "red");

        return style;
    }
    
    public static Dictionary<string, string> CreateStyle(params Tuple<string, string>[] tuples)
    {
        return tuples.ToDictionary(t => t.Item1, t => t.Item2);
    }
}