namespace Common.Core.Utils;

public static class TopicUtils
{
    public static string AsTopic(string configParam)
    {
        return "{" + configParam + "}";
    }
}