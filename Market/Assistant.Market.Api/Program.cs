using System.Reflection;
using System.Text.Json.Serialization;
using Assistant.Market.Infrastructure.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
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
    opt.AddPolicy("publishing", b => b.RequireRole("admin"));
});
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.ConfigureInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseAuthorization();

app.UseSwagger(options =>
{
    options.RouteTemplate = "Market/{documentName}/swagger.json";
});

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "Market";
    options.ConfigObject.PersistAuthorization = true;
});

app.MapControllers().RequireAuthorization();
/*
var secret = "helloworldimhere";
var issuer = builder.Configuration["Authentication:Schemes:Bearer:ValidIssuer"];

app.MapGet("/token", () =>
{
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    var tokenOptions = new JwtSecurityToken(
        issuer: issuer,
        audience: "http://localhost:8000",
        claims: new List<Claim>(),
        expires: DateTime.Now.AddMinutes(5),
        signingCredentials: signinCredentials
    );
    return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
}).RequireAuthorization();
*/
app.Run();