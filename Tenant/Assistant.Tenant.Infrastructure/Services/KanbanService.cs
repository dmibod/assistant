namespace Assistant.Tenant.Infrastructure.Services;

using Assistant.Tenant.Core.Services;
using KanbanApi.Client;
using Microsoft.Extensions.Logging;
using Board = Assistant.Tenant.Core.Services.Board;
using Card = Assistant.Tenant.Core.Services.Card;
using Lane = Assistant.Tenant.Core.Services.Lane;

public class KanbanService : IKanbanService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ITenantService tenantService;
    private readonly ILogger<KanbanService> logger;

    public KanbanService(IHttpClientFactory httpClientFactory, ITenantService tenantService, ILogger<KanbanService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.tenantService = tenantService;
        this.logger = logger;
    }
    
    private ApiClient ApiClient => new(this.httpClientFactory.CreateClient("KanbanApiClient"));

    public async Task<IEnumerable<string>> FindBoardIdsByOwnerAsync(string owner)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindBoardIdsByOwnerAsync), owner);

        var boards = await this.ApiClient.GetBoardsAsync(owner);

        return boards.Where(board => board.Owner == owner).Select(board => board.Id);
    }

    public async Task<IEnumerable<Board>> FindBoardsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindBoardsAsync));

        var tenant = await this.tenantService.GetOrCreateAsync();

        var boards = await this.ApiClient.GetBoardsAsync(tenant.Name);

        return boards.Select(board => new Board
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description
        });
    }

    public async Task<Board?> FindBoardAsync(string id)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindBoardAsync), id);

        var board = await this.ApiClient.GetBoardAsync(id);

        return board == null ? null : new Board
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description
        };
    }

    public async Task<Board> CreateBoardAsync(Board board)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateBoardAsync), board.Name);

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        var kanbanBoard =
            await this.ApiClient.CreateBoardAsync(new KanbanApi.Client.Board
            {
                Owner = tenant.Name, 
                Name = board.Name, 
                Description = board.Description, 
                Layout = LayoutTypes.H,
                Shared = false
            });

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

        await this.ApiClient.NameBoardAsync(kanbanBoard, board.Name);
        await this.ApiClient.DescribeBoardAsync(kanbanBoard, board.Description);
    }

    public Task RemoveBoardAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveBoardAsync), boardId);

        return this.ApiClient.RemoveBoardAsync(boardId);
    }

    public Task SetBoardLoadingStateAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetBoardLoadingStateAsync),
            boardId);

        return this.ApiClient.SetBoardLoadingStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        });
    }

    public Task SetBoardProgressStateAsync(string boardId, int progress)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetBoardProgressStateAsync),
            $"{boardId}-{progress}");

        return this.ApiClient.SetBoardProgressStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, progress);
    }

    public Task ResetBoardStateAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ResetBoardStateAsync), boardId);

        return this.ApiClient.ResetBoardStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        });
    }
    
    public async Task<IEnumerable<Lane>> FindBoardLanesAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindBoardLanesAsync), boardId);

        var lanes = await this.ApiClient.GetLanesAsync(new KanbanApi.Client.Board
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
            await this.ApiClient.CreateLaneAsync(new KanbanApi.Client.Board
            {
                Id = boardId
            }, boardId, lane.Name, lane.Description, LayoutTypes.V);

        return new Lane
        {
            Id = kanbanLane.Id,
            Name = kanbanLane.Name,
            Description = kanbanLane.Description
        };
    }
    
    public async Task<IEnumerable<Lane>> FindLanesAsync(string boardId, string parentLaneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindLanesAsync), $"{boardId}-{parentLaneId}");

        var lanes = await this.ApiClient.GetCardLanesAsync(new KanbanApi.Client.Board
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
            await this.ApiClient.CreateCardLaneAsync(new KanbanApi.Client.Board
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

    public async Task UpdateLaneAsync(string boardId, string laneId, string description)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateLaneAsync),
            $"{boardId}-{laneId}-{description}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        await this.ApiClient.DescribeLaneAsync(kanbanBoard, new KanbanApi.Client.Lane { Id = laneId }, description);
    }

    public async Task<IEnumerable<Card>> FindCardsAsync(string boardId, string cardLaneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindCardsAsync), $"{boardId}-{cardLaneId}");

        var cards = await this.ApiClient.GetCardsAsync(new KanbanApi.Client.Board
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
            await this.ApiClient.CreateCardAsync(new KanbanApi.Client.Board
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
    
    public async Task UpdateCardAsync(string boardId, string cardId, string description)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateCardAsync),
            $"{boardId}-{cardId}-{description}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        await this.ApiClient.DescribeCardAsync(kanbanBoard, new KanbanApi.Client.Card { Id = cardId }, description);
    }

    public async Task RemoveCardAsync(string boardId, string cardLaneId, string cardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveCardAsync),
            $"{boardId}-{cardLaneId}-{cardId}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        await this.ApiClient.RemoveCardAsync(kanbanBoard, new KanbanApi.Client.Card { Id = cardId }, cardLaneId);
    }

    public async Task RemoveBoardLaneAsync(string boardId, string laneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveBoardLaneAsync),
            $"{boardId}-{laneId}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        await this.ApiClient.RemoveBoardLaneAsync(kanbanBoard, new KanbanApi.Client.Lane { Id = laneId });
    }
}