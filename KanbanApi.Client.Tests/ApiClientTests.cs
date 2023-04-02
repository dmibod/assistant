namespace KanbanApi.Client.Tests;

[TestClass]
public class ApiClientTests
{
    private static readonly Uri ApiUri = new("http://localhost:8080/v1/api/");
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
        var boards = await client.GetBoardsAsync();

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
}