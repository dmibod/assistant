namespace Assistant.Tenant.Core.Services;

public interface IKanbanService
{
    Task<IEnumerable<string>> FindBoardIdsByOwnerAsync(string owner);
    
    Task<IEnumerable<Board>> FindBoardsAsync();

    Task<Board?> FindBoardAsync(string id);

    Task<Board> CreateBoardAsync(Board board);

    Task UpdateBoardAsync(Board board);

    Task RemoveBoardAsync(string boardId);

    Task SetBoardLoadingStateAsync(string boardId);

    Task SetBoardProgressStateAsync(string boardId, int progress);

    Task ResetBoardStateAsync(string boardId);
    
    Task<IEnumerable<Lane>> FindBoardLanesAsync(string boardId);

    Task<Lane> CreateBoardLaneAsync(string boardId, Lane lane);

    Task<IEnumerable<Lane>> FindLanesAsync(string boardId, string parentLaneId);
    
    Task<Lane> CreateCardLaneAsync(string boardId, string parentLaneId, Lane lane);
        
    Task UpdateLaneAsync(string boardId, string laneId, string description);

    Task<IEnumerable<Card>> FindCardsAsync(string boardId, string cardLaneId); 

    Task<Card> CreateCardAsync(string boardId, string cardLaneId, Card card);
        
    Task UpdateCardAsync(string boardId, string cardId, string description);

    Task RemoveCardAsync(string boardId, string cardLaneId, string cardId);
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