namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using Common.Core.Security;
using Common.Infrastructure.Security;
using KanbanApi.Client;
using Microsoft.Extensions.Logging;
using Board = Assistant.Market.Core.Services.Board;
using Card = Assistant.Market.Core.Services.Card;
using Lane = Assistant.Market.Core.Services.Lane;

public class KanbanService : IKanbanService
{
    public const string KanbanOwner = Identity.System;

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<KanbanService> logger;

    public KanbanService(IHttpClientFactory httpClientFactory, ILogger<KanbanService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    private ApiClient ApiClient => new(this.httpClientFactory.CreateClient("KanbanApiClient"));
    
    public async Task<IEnumerable<Board>> FindBoardsAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindBoardsAsync));

        var boards = await this.ApiClient.GetBoardsAsync(KanbanOwner);

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
            await this.ApiClient.CreateBoardAsync(KanbanOwner, board.Name, board.Description, LayoutTypes.V);

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

        return this.ApiClient.DescribeLaneAsync(kanbanBoard, new KanbanApi.Client.Lane { Id = lane.Id },
            lane.Description);
    }

    public Task RemoveLaneAsync(string boardId, string laneId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveLaneAsync),
            $"{boardId}-{laneId}");

        return this.ApiClient.RemoveLaneAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, new KanbanApi.Client.Lane
        {
            Id = laneId
        }, boardId);
    }

    public async Task<IEnumerable<Lane>> FindLanesAsync(string boardId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.FindLanesAsync), $"{boardId}");

        var lanes = await this.ApiClient.GetCardLanesAsync(new KanbanApi.Client.Board
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

    public async Task UpdateCardAsync(string boardId, Card card)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.UpdateCardAsync),
            $"{boardId}-{card.Id}-{card.Name}");

        var kanbanBoard = new KanbanApi.Client.Board
        {
            Id = boardId
        };

        await this.ApiClient.DescribeCardAsync(kanbanBoard, new KanbanApi.Client.Card { Id = card.Id }, card.Description);
    }

    public Task RemoveCardAsync(string boardId, string cardId, string parentId)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.RemoveCardAsync),
            $"{boardId}-{cardId}");

        return this.ApiClient.RemoveCardAsync(new KanbanApi.Client.Board
        {
            Id = boardId
        }, new KanbanApi.Client.Card
        {
            Id = cardId
        }, parentId);
    }
}