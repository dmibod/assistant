﻿namespace KanbanApi.Client.Tests;

[TestClass]
public class ApiClientTests
{
    private static readonly Uri ApiUri = new("https://dmitrybodnar.com/v1/api/");
    private const string Owner = "superuser";
    private static ApiClient client;

    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        var httpClient = new HttpClient();

        httpClient.BaseAddress = ApiUri;

        client = new ApiClient(httpClient);
    }

    [ClassCleanup]
    public static void TestFixtureTearDown()
    {
        client.Dispose();
    }

    [TestMethod]
    public async Task GetBoardsAsync_ReturnsExpectedResult()
    {
        // Arrange & Act
        var boards = await client.GetBoardsAsync(Owner);

        // Assert
        Assert.IsNotNull(boards);
    }

    [TestMethod]
    public async Task CreateBoardsAsync_ReturnsExpectedResult()
    {
        // Arrange && Act
        var board = await client.CreateBoardAsync(Owner, "Test name", "Test description", LayoutTypes.V);

        // Assert
        Assert.IsNotNull(board);
    }
    
    [TestMethod]
    public async Task RemoveBoardsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var name = Guid.NewGuid().ToString();
        var board = await client.CreateBoardAsync(Owner, name, string.Empty, LayoutTypes.V);
        
        // Act
        await client.RemoveBoardAsync(board.Id);

        // Assert
        var boards = await client.GetBoardsAsync(Owner);
        Assert.IsNull(boards.FirstOrDefault(board => board.Name == name));
    }
}