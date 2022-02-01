using Adapter.AuctionImageConversion;
using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.EfCore.ReadModelNotifications;
using Adapter.Hangfire_.Auctionhouse;
using Adapter.MongoDb;
using Adapter.MongoDb.AuctionImage;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Adapter.SqlServer.EventOutbox;
using AuctionBids.Application;
using Auctionhouse.Command;
using Auctionhouse.Command.Adapters;
using Auctions.Application;
using Categories.Domain;
using Common.Application;
using Common.Application.Events;
using Common.WebAPI;
using Common.WebAPI.Auth;
using IntegrationService.AuctionPaymentVerification;
using IntegrationService.CategoryNamesToTreeIdsConversion;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using QuartzTimeTaskService.AuctionEndScheduler;
using RabbitMq.EventBus;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using UserPayments.Application;
using Users.Application;
using XmlCategoryTreeStore;
using static System.Convert;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var moduleNames = new[] {
    "Auctions.Application", "UserPayments.Application", "AuctionBids.Application", "Users.Application"
};
var modules = moduleNames.Select(n => Assembly.Load(n)).ToArray();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

//MODULES
builder.Services.AddCommonCommandDependencies(modules);
builder.Services.AddAuctionBidsModule();
builder.Services.AddAuctionsModule();
builder.Services.AddCategoriesModule();
builder.Services.AddUserPaymentsModule();
builder.Services.AddUsersModule();

//ADAPTERS
builder.Services.AddWebApiAdapters();
builder.Services.AddAuctionImageConversion();
builder.Services.AddMongoDbImageDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddRabbitMq(builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>(),
    eventSubscriptionAssemblies: modules);
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddDapperAuctionhouse(builder.Configuration.GetSection("MSSql").Get<MsSqlConnectionSettings>());
builder.Services.AddQuartzTimeTaskServiceAuctionEndScheduler(builder.Configuration.GetSection("TimeTaskService").Get<TimeTaskServiceSettings>());
builder.Services.AddEfCoreReadModelNotifications(builder.Configuration.GetSection("EfCoreReadModelNotificatitons").Get<EfCoreReadModelNotificaitonsOptions>());
builder.Services.AddSqlServerEventOutboxStorage(builder.Configuration.GetSection("EventOutboxStorage")["ConnectionString"]);
builder.Services.AddHangfireServices(builder.Configuration.GetSection("HangfirePersistence")["ConnectionString"]);

//INTEGRATION SERVICES
builder.Services.AddAuctionPaymentVerification();
builder.Services.AddCategoryNamesToTreeIdsConversion();
//OUTBOX PROCESSOR BG SERVICE
builder.Services.AddOutboxProcessorService(new EventOutboxProcessorSettings
{
    MinMilisecondsDiff = ToInt32(builder.Configuration.GetSection(nameof(EventOutboxProcessorSettings))[nameof(EventOutboxProcessorSettings.MinMilisecondsDiff)]),
    EnableLogging = ToBoolean(builder.Configuration.GetSection(nameof(EventOutboxProcessorSettings))[nameof(EventOutboxProcessorSettings.EnableLogging)]),
});
//REDELIVERY SERVICE
builder.Services.AddSingleton(new EventBusSettings
{
    MaxRedelivery = Convert.ToInt32(builder.Configuration.GetSection(nameof(EventBusSettings))[nameof(EventBusSettings.MaxRedelivery)])
});
builder.Services.AddErrorEventRedeliveryProcessorService();

//WEB API SERVICES
//jwt auth
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddCommonJwtAuth(jwtConfig, authBuilder);
//logging and tracing
builder.Services.AddSerilogLogging(builder.Configuration, "Command");
Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;
builder.Services.AddTracing(b => {
    b.AddAspNetCoreInstrumentation();
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

var tracing = CommonInstaller.CreateModuleTracing("Command");

app.UseIdTokenManager();
app.UseIdTokenSlidingExpiration();
app.UseAuthentication();
app.UseStaticFiles();
app.UseSession();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
tracing.Dispose();