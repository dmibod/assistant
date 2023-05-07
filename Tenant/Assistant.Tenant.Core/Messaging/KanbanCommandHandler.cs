namespace Assistant.Tenant.Core.Messaging;

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