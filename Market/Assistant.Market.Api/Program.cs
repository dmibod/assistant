using Assistant.Market.Api.Services;
using Assistant.Market.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RefreshDataTimerService>();
builder.Services.AddHostedService<RefreshDataWorkerService>();
builder.Services.AddHostedService<MarketDataWorkerService>();
builder.Services.ConfigureInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();