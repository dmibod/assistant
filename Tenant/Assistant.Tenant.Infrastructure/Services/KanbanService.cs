namespace Assistant.Tenant.Infrastructure.Services;

using Assistant.Tenant.Core.Services;
using KanbanApi.Client;
using Microsoft.Extensions.Logging;
using Board = Assistant.Tenant.Core.Services.Board;
using Card = Assistant.Tenant.Core.Services.Card;
using Lane = Assistant.Tenant.Core.Services.Lane;

public class KanbanService : IKanbanService
{
    private readonly ITenantService tenantService;
    private readonly ApiClient apiClient;
    private readonly ILogger<KanbanService> logger;

    public KanbanService(ITenantService tenantService, ApiClient apiClient, ILogger<KanbanService> logger)
    {
        this.tenantService = tenantService;
        this.apiClient = apiClient;
        this.logger = logger;
    }

    public async Task<IEnumerable<Board>> FindBoardsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindBoardsAsync));

        var tenant = await this.tenantService.GetOrCreateAsync();

        var boards = await this.apiClient.GetBoardsAsync(tenant.Name);

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

        var tenant = await this.tenantService.GetOrCreateAsync();
        
        var kanbanBoard =
            await this.apiClient.CreateBoardAsync(new KanbanApi.Client.Board
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

    public Task SetBoardProgressStateAsync(string boardId, int progress)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.SetBoardProgressStateAsync),
            $"{boardId}-{progress}");

        return this.apiClient.SetBoardProgressStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, progress);
    }

    public Task ResetBoardStateAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ResetBoardStateAsync), boardId);

        return this.apiClient.ResetBoardStateAsync(new KanbanApi.Client.Board
        {
            Id = boardId
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
            }, boardId, lane.Name, lane.Description, LayoutTypes.V);

        return new Lane
        {
            Id = kanbanLane.Id,
            Name = kanbanLane.Name,
            Description = kanbanLane.Description
        };
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
}