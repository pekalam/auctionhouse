using Adapter.EfCore.ReadModelNotifications;
using Adapter.MongoDb;
using Auctionhouse.Query;
using Auctionhouse.Query.Adapters;
using Azure.Identity;
using Categories.DI;
using Common.DI;
using Common.Application.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RabbitMq.EventBus;
using ReadModel.Core;
using ReadModel.Core.Model;
using Serilog;
using WebAPI.Common;
using WebAPI.Common.Auth;
using WebAPI.Common.Configuration;
using WebAPI.Common.Tracing;
using ReadModel.DI;
using ReadModel.Contracts;

var builder = WebApplication.CreateBuilder(args);

var appConfigurationCs = ConfigurationUtils.GetAppConfigurationConnectionString(builder.Configuration);
var environmentName = ConfigurationUtils.GetEnvironmentName();
switch (environmentName)
{
    case ConfigurationUtils.LocalEnvName:
        break;
    case ConfigurationUtils.DockerEnvName:
        ConfigurationUtils.AddDockerAppSettings(builder.Host);
        break;
    default:
        ConfigurationUtils.InitializeAzureCredentials(builder.Configuration);
        builder.Host.ConfigureAppConfiguration(cfgBuilder =>
        {
            cfgBuilder.AddAzureAppConfiguration(cfg =>
            {
                cfg.Connect(appConfigurationCs)
                    .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()))
                    .Select("*", environmentName);
            });
        });
        break;
}

builder.Host.UseSerilog();

//MODULES
builder.Services.AddCommonQueryModule(builder.Configuration, 
    new[] { typeof(MongoDbReadModelInstaller).Assembly, typeof(ReadModelInstaller).Assembly }, new[] { typeof(MongoDbReadModelInstaller).Assembly });
builder.Services.AddCategoriesModule(builder.Configuration);
builder.Services.AddReadModelModule(builder.Configuration).AddQueryAdapters();

//EXTENSIONS
builder.Services.AddQueryEfCoreReadModelNotifications(builder.Configuration);

//WEB API SERVICES
//jwt auth
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddCommonJwtAuth(jwtConfig, authBuilder);
//logging and tracing
builder.Services.AddSerilogLogging(builder.Configuration, "Query");
builder.Services.AddTracing("Query", builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCacheServices(builder.Configuration);

var allowedOrigins = builder.Configuration.GetValue<string>("CORS:AllowedOrigins").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); ;
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(allowedOrigins);
    });
});


var app = builder.Build();

app.UseIdTokenManager();
app.UseIdTokenSlidingExpiration();
app.UseAuthentication();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthorization();

app.MapHub<ApplicationHub>("/app");
app.MapControllers();

app.Run();
