namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using KanbanApi.Client;
using Microsoft.Extensions.Logging;
using Board = Assistant.Market.Core.Services.Board;
using Card = Assistant.Market.Core.Services.Card;
using Lane = Assistant.Market.Core.Services.Lane;

public class KanbanService : IKanbanService
{
    public const string KanbanOwner = "test";

    private readonly ApiClient apiClient;
    private readonly ILogger<KanbanService> logger;

    public KanbanService(ApiClient apiClient, ILogger<KanbanService> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger;
    }

    public async Task<IEnumerable<Board>> FindBoardsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindBoardsAsync));

        var boards = await this.apiClient.GetBoardsAsync(KanbanOwner);

        return boards.Select(board => new Board
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description
        });
    }

    public async Task<Board> CreateBoardAsync(Board board)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateBoardAsync), board.Name);

        var kanbanBoard =
            await this.apiClient.CreateBoardAsync(KanbanOwner, board.Name, board.Description, LayoutTypes.V);

        return new Board
        {
            Id = kanbanBoard.Id,
            Name = kanbanBoard.Name,
            Description = kanbanBoard.Description
        };
    }

    public async Task UpdateBoardAsync(Board board)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateBoardAsync), board.Id);

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = board.Id
        };

        await this.apiClient.NameBoardAsync(kanbanBoard, board.Name);
        await this.apiClient.DescribeBoardAsync(kanbanBoard, board.Description);
    }

    public Task SetBoardLoadingStateAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetBoardLoadingStateAsync),
            boardId);

        return this.apiClient.SetBoardLoadingStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        });
    }

    public Task ResetBoardStateAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ResetBoardStateAsync), boardId);

        return this.apiClient.ResetBoardStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        });
    }

    public async Task<IEnumerable<Lane>> FindBoardLanesAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindBoardLanesAsync), boardId);

        var lanes = await this.apiClient.GetLanesAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        });

        return lanes.Select(lane => new Lane
        {
            Id = lane.Id,
            Name = lane.Name,
            Description = lane.Description
        });
    }

    public async Task<Lane> CreateBoardLaneAsync(string boardId, Lane lane)
    {
        this.logger.LogInformation("{Method} with arguments {Argument}", nameof(this.CreateBoardLaneAsync),
            $"{boardId}-{lane.Name}");

        var kanbanLane =
            await this.apiClient.CreateLaneAsync(new KanbanApi.Client.Board
            {
                Id = boardId
            }, boardId, lane.Name, lane.Description, LayoutTypes.H);

        return new Lane
        {
            Id = kanbanLane.Id,
            Name = kanbanLane.Name,
            Description = kanbanLane.Description
        };
    }

    public Task UpdateBoardLaneAsync(string boardId, Lane lane)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateBoardLaneAsync),
            $"{boardId}-{lane.Name}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        return this.apiClient.DescribeLaneAsync(kanbanBoard, new KanbanApi.Client.Lane { Id = lane.Id },
            lane.Description);
    }

    public Task RemoveBoardLaneAsync(string boardId, string laneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveBoardLaneAsync),
            $"{boardId}-{laneId}");

        return this.apiClient.RemoveLaneAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, new KanbanApi.Client.Lane
        {
            Id = laneId
        }, boardId);
    }

    public async Task<IEnumerable<Lane>> FindLanesAsync(string boardId, string parentLaneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindLanesAsync), $"{boardId}-{parentLaneId}");

        var lanes = await this.apiClient.GetCardLanesAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, new KanbanApi.Client.Lane
        {
            Id = parentLaneId
        });

        return lanes.Select(lane => new Lane
        {
            Id = lane.Id,
            Name = lane.Name,
            Description = lane.Description
        });
    }

    public async Task<Lane> CreateCardLaneAsync(string boardId, string parentLaneId, Lane lane)
    {
        this.logger.LogInformation("{Method} with arguments {Argument}", nameof(this.CreateCardLaneAsync),
            $"{boardId}-{parentLaneId}-{lane.Name}");

        var kanbanLane =
            await this.apiClient.CreateCardLaneAsync(new KanbanApi.Client.Board
            {
                Id = boardId
            }, parentLaneId, lane.Name, lane.Description);

        return new Lane
        {
            Id = kanbanLane.Id,
            Name = kanbanLane.Name,
            Description = kanbanLane.Description
        };
    }

    public async Task<IEnumerable<Card>> FindCardsAsync(string boardId, string cardLaneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindCardsAsync), $"{boardId}-{cardLaneId}");

        var cards = await this.apiClient.GetCardsAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, new CardLane
        {
            Id = cardLaneId
        });

        return cards.Select(lane => new Card
        {
            Id = lane.Id,
            Name = lane.Name,
            Description = lane.Description
        });
    }

    public async Task<Card> CreateCardAsync(string boardId, string cardLaneId, Card card)
    {
        this.logger.LogInformation("{Method} with arguments {Argument}", nameof(this.CreateCardAsync),
            $"{boardId}-{cardLaneId}-{card.Name}");

        var kanbanCard =
            await this.apiClient.CreateCardAsync(new KanbanApi.Client.Board
            {
                Id = boardId
            }, new CardLane
            {
                Id = cardLaneId
            }, card.Name, card.Description);

        return new Card
        {
            Id = kanbanCard.Id,
            Name = kanbanCard.Name,
            Description = kanbanCard.Description
        };
    }

    public async Task UpdateCardAsync(string boardId, Card card)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateCardAsync),
            $"{boardId}-{card.Id}-{card.Description}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        await this.apiClient.DescribeCardAsync(kanbanBoard, new KanbanApi.Client.Card { Id = card.Id }, card.Description);
    }
}