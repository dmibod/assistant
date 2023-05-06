namespace Common.Core.Messaging;

public interface ITopicResolver
{
    string Resolve(string topic);
}