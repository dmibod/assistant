namespace KanbanApi.Client;

using System.Net.Http.Json;
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
    
    public async Task<IEnumerable<Board>?> GetBoardsAsync(string? owner = null)
    {
        using var response = await this.httpClient.GetAsync(string.IsNullOrEmpty(owner) ? "board" : $"board?owner={owner}");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Board[]>(SerializationDefaults.Options);
    }
    
    public async Task<Board?> GetBoardAsync(string id)
    {
        using var response = await this.httpClient.GetAsync($"board/{id}");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Board>(SerializationDefaults.Options);
    }

    public async Task<Board?> CreateBoardAsync(Board board)
    {
        using var response = await this.httpClient.PostAsJsonAsync("board", board, SerializationDefaults.Options);

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

    public Task SetBoardLoadingStateAsync(Board board)
    {
        var command = CommandTypes.StateBoardCommand
            .BoardCommand(board)
            .WithPayload("state", "loading");

        return this.CommandAsync(command);
    }

    public Task ResetBoardLoadingStateAsync(Board board)
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
        using var response = await this.httpClient.PutAsJsonAsync($"board/{board.Id}/share", new { shared = value });

        response.EnsureSuccessStatusCode();
        
        return await response.AsJsonAsync<Board>(SerializationDefaults.Options);
    }

    #endregion
    #region Lane Api

    public async Task<IEnumerable<Lane>?> GetLanesAsync(Board board)
    {
        using var response = await this.httpClient.GetAsync($"board/{board.Id}/lanes");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Lane[]>(SerializationDefaults.Options);
    }

    public async Task<IEnumerable<Lane>?> GetLanesAsync(Board board, Lane lane)
    {
        using var response = await this.httpClient.GetAsync($"board/{board.Id}/lanes/{lane.Id}/lanes");

        response.EnsureSuccessStatusCode();

        return await response.AsJsonAsync<Lane[]>(SerializationDefaults.Options);
    }

    public Task DescribeLaneAsync(Board board, Lane lane, string description)
    {
        var command = CommandTypes.DescribeLaneCommand
            .LaneCommand(board, lane)
            .WithPayload("description", description);

        return this.CommandAsync(command);
    }
    
    public Task<Lane> CreateLaneAsync(Board board, string parentId, string name, string description, LayoutTypes layout)
    {
        return this.CreateLaneAsync(board, parentId, LaneTypes.L, name, description, layout);
    }

    public Task<Lane> CreateCardLaneAsync(Board board, string parentId, string name, string description)
    {
        return this.CreateLaneAsync(board, parentId, LaneTypes.C, name, description, LayoutTypes.H);
    }

    private async Task<Lane> CreateLaneAsync(Board board, string parentId, LaneTypes type, string name, string description, LayoutTypes layout)
    {
        var request = new Lane
        {
            Name = name, 
            Description = description, 
            Type = type, 
            Layout = layout
        };
        
        using var response = await this.httpClient.PostAsJsonAsync($"board/{board.Id}/lanes", request, SerializationDefaults.Options);

        response.EnsureSuccessStatusCode();
        
        var lane = await response.AsJsonAsync<Lane>(SerializationDefaults.Options);

        await this.AttachAsync(board.Id, lane.Id, parentId);
        
        return lane;
    }

    #endregion
    #region Card Api
    
    public async Task<IEnumerable<Card>?> GetCardsAsync(Board board, Lane lane)
    {
        using var response = await this.httpClient.GetAsync($"board/{board.Id}/lanes/{lane.Id}/cards");

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

    public async Task<Card> CreateCardAsync(Board board, Lane lane, string name, string description)
    {
        var request = new Card
        {
            Name = name, 
            Description = description
        };
        
        using var response = await this.httpClient.PostAsJsonAsync($"board/{board.Id}/cards", request);

        response.EnsureSuccessStatusCode();
        
        var card = await response.AsJsonAsync<Card>(SerializationDefaults.Options);

        await this.AttachAsync(board.Id, card.Id, lane.Id);
        
        return card;
    }

    #endregion
    #region Common Api
    
    public async Task<HttpResponseMessage> CommandAsync(Command command)
    {
        using var response = await this.httpClient.PostAsJsonAsync($"command/{command.BoardId}", new[] { command }, SerializationDefaults.Options);

        response.EnsureSuccessStatusCode();

        return response;
    }

    private async Task<CommandResponse?> AttachAsync(string boardId, string id, string parentId)
    {
        var command = CommandTypes.AppendChildCommand
            .Command(boardId, id)
            .WithPayload("parent_id", parentId);

        var response = await this.CommandAsync(command);
        
        return await response.AsJsonAsync<CommandResponse>(SerializationDefaults.Options);
    }
    
    #endregion

    public void Dispose()
    {
        this.httpClient.Dispose();
    }
}