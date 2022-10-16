using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.EfCore.ReadModelNotifications;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Auctionhouse.Command;
using Auctionhouse.Command.Controllers;
using Auctionhouse.Command.ModuleInstallation;
using Azure.Identity;
using ChronicleEfCoreStorage;
using Common.Application;
using Common.Application.Events;
using Common.WebAPI;
using Common.WebAPI.Auth;
using Common.WebAPI.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using RabbitMq.EventBus;
using Serilog;
using System.Diagnostics;
using System.Reflection;

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

var moduleNames = new[] {
    "Auctions.Application", "UserPayments.Application", "AuctionBids.Application", "Users.Application"
};
var modules = moduleNames.Select(n => Assembly.Load(n)).ToArray();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

//MODULES
builder.Services.AddAuctionsModule(builder.Configuration);
builder.Services.AddCommonModule(builder.Configuration, modules);
builder.Services.AddCategoriesModule(builder.Configuration);
builder.Services.AddAuctionBidsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddUserPaymentsModule(builder.Configuration);

builder.Services.AddChronicleSQLServerStorage(SagaTypeSerialization.GetSagaType, builder.Configuration.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>().ConnectionString);

//EXTENSIONS
builder.Services.AddCommandEfCoreReadModelNotifications(builder.Configuration);


//DEMO MODE
builder.Services.AddOptions<DemoModeOptions>().Bind(builder.Configuration.GetSection("DemoMode"));

//REDELIVERY SERVICE
builder.Services.AddErrorEventRedeliveryProcessorService(new EventBusSettings
{
    MaxRedelivery = Convert.ToInt32(builder.Configuration.GetSection(nameof(EventBusSettings))[nameof(EventBusSettings.MaxRedelivery)])
});

//WEB API SERVICES
//jwt auth
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddCommonJwtAuth(jwtConfig, authBuilder);
//logging and tracing
builder.Services.AddSerilogLogging(builder.Configuration, "Command");
Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;
builder.Services.AddTracing(b =>
{
    //b.AddAspNetCoreInstrumentation();
});

builder.Services.AddCacheServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllers()
    //ADAPTER
    .AddQuartzTimeTaskServiceAuctionEndSchedulerServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

var tracing = TracingInstaller.CreateModuleTracing("Command");

app.UseIdTokenManager();
app.UseIdTokenSlidingExpiration();
app.UseAuthentication();
app.UseStaticFiles();
app.UseSession();
app.UseMiddleware<DemoModeMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
tracing.Dispose();