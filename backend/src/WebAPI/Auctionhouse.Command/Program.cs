using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.EfCore.ReadModelNotifications;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using AuctionBids.DI;
using Auctionhouse.Command;
using Auctionhouse.Command.Adapters;
using Auctionhouse.Command.Controllers;
using Auctions.DI;
using Azure.Identity;
using Categories.DI;
using ChronicleEfCoreStorage;
using Common.DI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using UserPayments.DI;
using Users.DI;
using WebAPI.Common;
using WebAPI.Common.Auth;
using WebAPI.Common.Configuration;
using WebAPI.Common.Tracing;

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
builder.Services.AddAuctionsModule(builder.Configuration).AddCommandAdapters();
builder.Services.AddCommonCommandModule(builder.Configuration, modules);
builder.Services.AddCategoriesModule(builder.Configuration);
builder.Services.AddAuctionBidsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration).AddCommandAdapters();
builder.Services.AddUserPaymentsModule(builder.Configuration);

builder.Services.AddChronicleSQLServerStorage(builder.Configuration.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>().ConnectionString);

//EXTENSIONS
builder.Services.AddCommandEfCoreReadModelNotifications(builder.Configuration);

//DEMO MODE
builder.Services.AddOptions<DemoModeOptions>().Bind(builder.Configuration.GetSection("DemoMode"));

//WEB API SERVICES
//jwt auth
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var authBuilder = builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddCommonJwtAuth(jwtConfig, authBuilder);
//logging and tracing
builder.Services.AddSerilogLogging(builder.Configuration, "Command");
Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;
builder.Services.AddTracing("Command", builder.Configuration);

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
AuctionEndSchedulerInstaller.AddQuartzTimeTaskServiceWebApiServices(builder.Services.AddControllers(), authBuilder);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


app.UseIdTokenManager();
app.UseIdTokenSlidingExpiration();
app.UseAuthentication();
app.UseStaticFiles();
app.UseSession();
app.UseMiddleware<DemoModeMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();