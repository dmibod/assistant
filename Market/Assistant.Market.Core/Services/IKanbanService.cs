namespace Assistant.Market.Core.Services;

public interface IKanbanService
{
    Task<IEnumerable<Board>> FindBoardsAsync();

    Task<Board> CreateBoardAsync(Board board);

    Task UpdateBoardAsync(Board board);

    Task SetBoardLoadingStateAsync(string boardId);
    
    Task SetBoardProgressStateAsync(string boardId, int progress);
    
    Task ResetBoardStateAsync(string boardId);
    
    Task<IEnumerable<Lane>> FindBoardLanesAsync(string boardId);
    
    Task<Lane> CreateBoardLaneAsync(string boardId, Lane lane);

    Task UpdateBoardLaneAsync(string boardId, Lane lane);

    Task RemoveLaneAsync(string boardId, string laneId);
    
    Task<IEnumerable<Lane>> FindLanesAsync(string boardId);
    
    Task<IEnumerable<Lane>> FindLanesAsync(string boardId, string parentLaneId);
    
    Task<Lane> CreateCardLaneAsync(string boardId, string parentLaneId, Lane lane);
    
    Task<IEnumerable<Card>> FindCardsAsync(string boardId, string cardLaneId); 
    
    Task<Card> CreateCardAsync(string boardId, string cardLaneId, Card card);
    
    Task UpdateCardAsync(string boardId, Card card);
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