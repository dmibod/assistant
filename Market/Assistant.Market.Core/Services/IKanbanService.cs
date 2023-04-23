namespace Assistant.Market.Core.Services;

public interface IKanbanService
{
    Task<IEnumerable<Board>> FindBoardsAsync();

    Task<Board> CreateBoardAsync(Board board);

    Task UpdateBoardAsync(Board board);

    Task SetBoardLoadingStateAsync(string boardId);

    Task ResetBoardStateAsync(string boardId);
    
    Task<IEnumerable<Lane>> FindBoardLanesAsync(string boardId);
    
    Task<Lane> CreateBoardLaneAsync(string boardId, Lane lane);

    Task UpdateBoardLaneAsync(string boardId, Lane lane);

    Task RemoveBoardLaneAsync(string boardId, string laneId);
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