using System.Reflection;
using System.Text.Json.Serialization;
using Assistant.Tenant.Infrastructure.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.WithOrigins("*")
                //.SetIsOriginAllowedToAllowWildcardSubdomains()
                //.AllowCredentials()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
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
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("administration", b => b.RequireRole("admin"));
});
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.ConfigureInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors();
app.UseAuthorization();

app.UseSwagger(options =>
{
    options.RouteTemplate = "Tenant/{documentName}/swagger.json";
});

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "Tenant";
    options.ConfigObject.PersistAuthorization = true;
});

app.MapControllers().RequireAuthorization();
app.Run();