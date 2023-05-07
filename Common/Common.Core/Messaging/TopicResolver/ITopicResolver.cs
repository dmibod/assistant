namespace Common.Core.Messaging.TopicResolver;

public interface ITopicResolver
{
    string Resolve(string topic);
}