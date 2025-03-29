using BloodBowl.Api;
using Microsoft.EntityFrameworkCore;
using BloodBowl.Domain.Context;
using System.Reflection;
using BloodBowl.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddDbContext<BloodBowlDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IPlayerScoreService, PlayerScoreService>();
builder.Services.AddSingleton<GameRoom>();

builder.Services.AddControllers();

builder.Services.AddSignalR();
builder.WebHost.UseUrls(builder.Configuration.GetValue<string>("ServerSettings:HostUrl")!);
var app = builder.Build();
app.UseRouting();
app.UseCors("CorsPolicy");
app.MapHub<GameHub>("/gameHub");

app.Run();
