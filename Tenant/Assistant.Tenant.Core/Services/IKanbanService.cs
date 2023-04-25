namespace Assistant.Tenant.Core.Services;

public interface IKanbanService
{
    Task<IEnumerable<Board>> FindBoardsAsync();

    Task<Board> CreateBoardAsync(Board board);

    Task UpdateBoardAsync(Board board);

    Task SetBoardLoadingStateAsync(string boardId);

    Task SetBoardProgressStateAsync(string boardId, int progress);

    Task ResetBoardStateAsync(string boardId);
    
    Task<Lane> CreateBoardLaneAsync(string boardId, Lane lane);
    
    Task<Lane> CreateCardLaneAsync(string boardId, string parentLaneId, Lane lane);
    
    Task<Card> CreateCardAsync(string boardId, string cardLaneId, Card card);
}

public abstract class Entity
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
}

public class Board : Entity
{
}

public class Lane : Entity
{
}

public class Card : Entity
{
}