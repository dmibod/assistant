namespace Common.Core.Messaging.TopicResolver;

public class MapTopicResolver : ITopicResolver
{
    private readonly IDictionary<string, string> map;

    public MapTopicResolver(IDictionary<string, string> map)
    {
        this.map = map;
    }

    public string Resolve(string topic)
    {
        return this.map.TryGetValue(topic, out var value) ? value : topic;
    }
}