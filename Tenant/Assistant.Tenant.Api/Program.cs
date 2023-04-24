using Assistant.Tenant.Api.Services;
using Assistant.Tenant.Core.Security;
using Assistant.Tenant.Infrastructure.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddSingleton<IIdentityProvider, IdentityProvider>();
builder.Services.ConfigureInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseAuthorization();

app.UseSwagger(options =>
{
    options.RouteTemplate = "Tenant/{documentName}/swagger.json";
});

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "Tenant";
});

app.MapControllers().RequireAuthorization();
app.Run();