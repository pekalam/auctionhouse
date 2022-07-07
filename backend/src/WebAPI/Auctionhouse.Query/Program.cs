using Adapter.EfCore.ReadModelNotifications;
using Adapter.MongoDb;
using Auctionhouse.Query;
using Auctionhouse.Query.Adapters;
using Azure.Identity;
using Categories.Domain;
using Common.Application;
using Common.WebAPI;
using Common.WebAPI.Auth;
using Common.WebAPI.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Trace;
using RabbitMq.EventBus;
using ReadModel.Core;
using ReadModel.Core.Model;
using ReadModel.Core.Services;
using Serilog;
using XmlCategoryTreeStore;

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
builder.Services.AddCommonQueryDependencies(typeof(ReadModelInstaller).Assembly);
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
builder.Services.AddReadModel(mongoDbSettings);
builder.Services.AddCategoriesModule();

//ADAPTERS
builder.Services.AddRabbitMq(builder.Configuration, eventConsumerAssemblies: new[] { typeof(ReadModelInstaller).Assembly });
builder.Services.AddXmlCategoryTreeStore(builder.Configuration);
builder.Services.AddMongoDbImageDb(builder.Configuration);
builder.Services.AddEfCoreReadModelNotifications(builder.Configuration);
builder.Services.AddTransient<IBidRaisedNotifications, BidRaisedNotifications>();

//WEB API SERVICES
//jwt auth
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddCommonJwtAuth(jwtConfig, authBuilder);
//logging and tracing
builder.Services.AddSerilogLogging(builder.Configuration, "Query");
builder.Services.AddTracing(b =>
{
    //b.AddAspNetCoreInstrumentation();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
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

var tracing = CommonInstaller.CreateModuleTracing("Query");

app.UseIdTokenManager();
app.UseIdTokenSlidingExpiration();
app.UseAuthentication();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapHub<ApplicationHub>("/app");
app.MapControllers();

app.Run();
tracing.Dispose();