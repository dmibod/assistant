namespace Assistant.Tenant.Core.Messaging;

using System.Text.Json.Serialization;
using Common.Core.Messaging;
using Common.Core.Messaging.Attributes;
using Microsoft.Extensions.Logging;

[Handler("command", enabled: false)]
public class KanbanCommandHandler : IMessageHandler<List<KanbanCommand>>
{
    private readonly ILogger<KanbanCommandHandler> logger;

    public KanbanCommandHandler(ILogger<KanbanCommandHandler> logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(List<KanbanCommand> commands)
    {
        this.logger.LogInformation("Received kanban commands {Count}", commands.Count);

        foreach (var command in commands)
        {
            this.logger.LogInformation("Received kanban command '{Type}' for '{Entity}' of '{Board}'", 
                command.CommandType,
                command.EntityId, 
                command.BoardId);
        }
        
        return Task.CompletedTask;
    }
}

public enum KanbanCommandType
{
    UpdateCardCommand,
    RemoveCardCommand,
    UpdateLaneCommand,
    RemoveLaneCommand,
    ExcludeChildCommand,
    AppendChildCommand,
    InsertBeforeCommand,
    InsertAfterCommand,
    LayoutBoardCommand,
    LayoutLaneCommand,
    DescribeBoardCommand,
    DescribeLaneCommand,
    DescribeCardCommand,
    UpdateBoardCommand,
    StateBoardCommand
}

public class KanbanCommand
{
    [JsonPropertyName("id")]
    public string EntityId { get; set; }
    
    [JsonPropertyName("board_id")]
    public string BoardId { get; set; }

    [JsonPropertyName("type")]
    public KanbanCommandType CommandType { get; set; }

    [JsonPropertyName("payload")]
    public IDictionary<string, string> Payload { get; set; }
}