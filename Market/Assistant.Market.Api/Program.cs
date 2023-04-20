using Assistant.Market.Api.Configuration;
using Assistant.Market.Api.Services;
using Assistant.Market.Core.Services;
using Assistant.Market.Infrastructure.Services;
using NATS.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<FeedTimerService>();
builder.Services.AddHostedService<FeedWorkerService>();
builder.Services.AddNatsClient(options =>
{
    var settings = builder.Configuration.GetSection("NatsSettings").Get<NatsSettings>();
    options.User = settings.User;
    options.Password = settings.Password;
    options.Url = settings.Url;
});
builder.Services.AddHttpClient<PolygonApi.Client.ApiClient>("PolygonApiClient");
builder.Services.AddHttpClient<KanbanApi.Client.ApiClient>("KanbanApiClient", client =>
{
    client.BaseAddress = new Uri("https://dmitrybodnar.com/v1/api/");
});
builder.Services.AddSingleton<IFeedService, FeedService>();
builder.Services.AddSingleton<IStockService, StockService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();