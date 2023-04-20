using Assistant.Market.Api.Services;
using Assistant.Market.Infrastructure.Configuration;
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
builder.Services.ConfigureInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();