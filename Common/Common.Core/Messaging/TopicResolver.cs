namespace Common.Core.Messaging;

public class TopicResolver : ITopicResolver
{
    private readonly IDictionary<string, string> map;

    public TopicResolver(IDictionary<string, string> map)
    {
        this.map = map;
    }

    public string Resolve(string topic)
    {
        return this.map.TryGetValue(topic, out var value) ? value : topic;
    }
}