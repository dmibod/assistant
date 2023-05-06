namespace Common.Core.Messaging;

[AttributeUsage(AttributeTargets.Class)]
public class HandlerAttribute : Attribute
{
    public HandlerAttribute(string topic)
    {
        this.Topic = topic;
        this.Enabled = true;
    }

    public HandlerAttribute(string topic, bool enabled)
    {
        this.Topic = topic;
        this.Enabled = enabled;
    }

    public string Topic { get; }
    
    public bool Enabled { get; }
}