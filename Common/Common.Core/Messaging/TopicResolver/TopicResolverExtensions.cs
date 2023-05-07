namespace Common.Core.Messaging.TopicResolver;

using Common.Core.Utils;

public static class TopicResolverExtensions
{
    public static string ResolveConfig(this ITopicResolver resolver, string configParam)
    {
        return resolver.Resolve(TopicUtils.AsTopic(configParam));
    }
}