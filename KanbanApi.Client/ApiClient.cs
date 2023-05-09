namespace KanbanApi.Client;

using System.Net;
using System.Net.Http.Json;
using KanbanApi.Client.Abstract;
using KanbanApi.Client.Extensions;
using KanbanApi.Client.Serialization;

public class ApiClient : IDisposable
{
    private readonly HttpClient httpClient;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    #region Board Api
    
    public async Task<IEnumerable<Board>?> GetBoardsAsync(string? owner)
    {
        var requestUri = string.IsNullOrEmpty(owner) 
            ? "/v1/api/board" 
            : $"/v1/api/board?owner={owner}";

        using var response = await this.httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Board[]>(SerializationDefaults.Options);
    }
    
    public async Task<Board?> GetBoardAsync(string id)
    {
        using var response = await this.httpClient.GetAsync($"/v1/api/board/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Board>(SerializationDefaults.Options);
    }

    public async Task<Board?> CreateBoardAsync(Board board)
    {
        using var response = await this.httpClient.PostAsJsonAsync("/v1/api/board", board, SerializationDefaults.Options);

        response.EnsureSuccessStatusCode();
        
        return await response.AsJsonAsync<Board>(SerializationDefaults.Options);
    }

    public Task<Board?> CreateBoardAsync(string owner, string name, string description, LayoutTypes layout)
    {
        var board = new Board
        {
            Name = name, 
            Description = description, 
            Owner = owner, 
            Layout = layout, 
            Shared = true
        };

        return this.CreateBoardAsync(board);
    }

    public async Task RemoveBoardAsync(string id)
    {
        using var response = await this.httpClient.DeleteAsync($"/v1/api/board/{id}");

        response.EnsureSuccessStatusCode();
    }

    public Task SetBoardLoadingStateAsync(Board board)
    {
        var command = CommandTypes.StateBoardCommand
            .BoardCommand(board)
            .WithPayload("state", "loading");

        return this.CommandAsync(command);
    }

    public Task SetBoardProgressStateAsync(Board board, int progress)
    {
        var command = CommandTypes.StateBoardCommand
            .BoardCommand(board)
            .WithPayload("state", $"progress:{progress}");

        return this.CommandAsync(command);
    }

    public Task ResetBoardStateAsync(Board board)
    {
        var command = CommandTypes.StateBoardCommand
            .BoardCommand(board)
            .WithPayload("state", string.Empty);
        
        return this.CommandAsync(command);
    }

    public Task NameBoardAsync(Board board, string name)
    {
        var command = CommandTypes.UpdateBoardCommand
            .BoardCommand(board)
            .WithPayload("name", name);
        
        return this.CommandAsync(command);
    }

    public Task DescribeBoardAsync(Board board, string description)
    {
        var command = CommandTypes.DescribeBoardCommand
            .BoardCommand(board)
            .WithPayload("description", description);
        
        return this.CommandAsync(command);
    }
    
    public async Task<Board?> ShareBoardAsync(Board board, bool value)
    {
        using var response = await this.httpClient.PutAsJsonAsync($"/v1/api/board/{board.Id}/share", new { shared = value });

        response.EnsureSuccessStatusCode();
        
        return await response.AsJsonAsync<Board>(SerializationDefaults.Options);
    }

    #endregion
    #region Lane Api

    public Task<IEnumerable<Lane>?> GetLanesAsync(Board board)
    {
        return this.GetLanesAsync<Lane>(board.Id);
    }

    public Task<IEnumerable<Lane>?> GetLanesAsync(Board board, Lane lane)
    {
        return this.GetLanesAsync<Lane>(board, lane.Id);
    }

    public Task<IEnumerable<CardLane>?> GetCardLanesAsync(Board board)
    {
        return this.GetLanesAsync<CardLane>(board.Id);
    }

    public Task<IEnumerable<CardLane>?> GetCardLanesAsync(Board board, Lane lane)
    {
        return this.GetLanesAsync<CardLane>(board, lane.Id);
    }

    private async Task<IEnumerable<T>?> GetLanesAsync<T>(string boardId) where T : BaseLane
    {
        using var response = await this.httpClient.GetAsync($"/v1/api/board/{boardId}/lanes");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<T[]>(SerializationDefaults.Options);
    }

    private async Task<IEnumerable<T>?> GetLanesAsync<T>(Board board, string parentId) where T : BaseLane
    {
        using var response = await this.httpClient.GetAsync($"/v1/api/board/{board.Id}/lanes/{parentId}/lanes");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<T[]>(SerializationDefaults.Options);
    }

    public Task DescribeLaneAsync(Board board, BaseLane lane, string description)
    {
        var command = CommandTypes.DescribeLaneCommand
            .LaneCommand(board, lane)
            .WithPayload("description", description);

        return this.CommandAsync(command);
    }
    
    public Task RemoveBoardLaneAsync(Board board, Lane lane)
    {
        return this.RemoveLaneAsync(board, lane, board.Id);
    }

    public Task RemoveLaneAsync(Board board, Lane lane, string parentId)
    {
        var command = CommandTypes.RemoveLaneCommand
            .LaneCommand(board, lane)
            .WithPayload("parent_id", parentId);

        return this.CommandAsync(command);
    }
    
    public Task<Lane> CreateLaneAsync(Board board, string parentId, string name, string description, LayoutTypes layout)
    {
        var request = new Lane
        {
            Name = name, 
            Description = description, 
            Type = LaneTypes.L, 
            Layout = layout
        };

        return this.CreateLaneAsync(board, parentId, request);
    }

    public Task<CardLane> CreateCardLaneAsync(Board board, string parentId, string name, string description)
    {
        var request = new CardLane
        {
            Name = name, 
            Description = description, 
            Type = LaneTypes.C
        };

        return this.CreateLaneAsync(board, parentId, request);
    }

    private async Task<T> CreateLaneAsync<T>(Board board, string parentId, T request) where T : BaseLane
    {
        using var response = await this.httpClient.PostAsJsonAsync($"/v1/api/board/{board.Id}/lanes", request, SerializationDefaults.Options);

        response.EnsureSuccessStatusCode();
        
        var lane = await response.AsJsonAsync<T>(SerializationDefaults.Options);

        await this.AttachAsync(board.Id, lane.Id, parentId);
        
        return lane;
    }

    #endregion
    #region Card Api
    
    public async Task<IEnumerable<Card>?> GetCardsAsync(Board board, CardLane lane)
    {
        using var response = await this.httpClient.GetAsync($"/v1/api/board/{board.Id}/lanes/{lane.Id}/cards");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Card[]>(SerializationDefaults.Options);
    }

    public Task DescribeCardAsync(Board board, Card card, string description)
    {
        var command = CommandTypes.DescribeCardCommand
            .CardCommand(board, card)
            .WithPayload("description", description);

        return this.CommandAsync(command);
    }

    public async Task<Card> CreateCardAsync(Board board, CardLane lane, string name, string description)
    {
        var request = new Card
        {
            Name = name, 
            Description = description
        };
        
        using var response = await this.httpClient.PostAsJsonAsync($"/v1/api/board/{board.Id}/cards", request);

        response.EnsureSuccessStatusCode();
        
        var card = await response.AsJsonAsync<Card>(SerializationDefaults.Options);

        await this.AttachAsync(board.Id, card.Id, lane.Id);
        
        return card;
    }

    public Task RemoveCardAsync(Board board, Card card, string parentId)
    {
        var command = CommandTypes.RemoveCardCommand
            .CardCommand(board, card)
            .WithPayload("parent_id", parentId);

        return this.CommandAsync(command);
    }

    #endregion
    #region Common Api
    
    public async Task<CommandResponse?> CommandAsync(Command command)
    {
        using var response = await this.httpClient.PostAsJsonAsync($"/v1/api/command/{command.BoardId}", new[] { command }, SerializationDefaults.Options);

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<CommandResponse>(SerializationDefaults.Options);
    }

    private async Task<CommandResponse?> AttachAsync(string boardId, string id, string parentId)
    {
        var command = CommandTypes.AppendChildCommand
            .Command(boardId, id)
            .WithPayload("parent_id", parentId);

        return await this.CommandAsync(command);
    }
    
    #endregion

    public void Dispose()
    {
        this.httpClient.Dispose();
    }
}