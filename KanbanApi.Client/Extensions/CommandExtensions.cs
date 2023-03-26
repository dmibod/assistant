namespace KanbanApi.Client.Extensions;

public static class CommandExtensions
{
    public static Command Command(this CommandTypes type, string boardId, string entityId)
    {
        return new Command
        {
            Id = entityId,
            BoardId = boardId,
            CommandType = (int)type
        };
    }

    public static Command BoardCommand(this CommandTypes type, Board board)
    {
        return new Command
        {
            Id = board.Id,
            BoardId = board.Id,
            CommandType = (int)type
        };
    }
    
    public static Command LaneCommand(this CommandTypes type, Board board, Lane lane)
    {
        return new Command
        {
            Id = lane.Id,
            BoardId = board.Id,
            CommandType = (int)type
        };
    }

    public static Command CardCommand(this CommandTypes type, Board board, Card card)
    {
        return new Command
        {
            Id = card.Id,
            BoardId = board.Id,
            CommandType = (int)type
        };
    }

    public static Command WithPayload(this Command command, string key, string value)
    {
        command.Payload.Add(key, value);
        
        return command;
    }
}