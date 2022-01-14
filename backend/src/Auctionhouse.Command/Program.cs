using Adapter.AuctionImageConversion;
using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.EfCore.ReadModelNotifications;
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
using Microsoft.OpenApi.Models;
using QuartzTimeTaskService.AuctionEndScheduler;
using RabbitMq.EventBus;
using System.Reflection;
using XmlCategoryTreeStore;

var builder = WebApplication.CreateBuilder(args);

var moduleNames = new[] {
    "Auctions.Application", "UserPayments.Application", "AuctionBids.Application", "Users.Application"
};
var modules = moduleNames.Select(n => Assembly.Load(n)).ToArray();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSingleton(builder.Configuration.GetSection("JWT").Get<JwtSettings>());
// Add services to the container
builder.Services.AddCommon(modules);
builder.Services.AddAuctionBidsModule();
builder.Services.AddAuctionsModule(moduleNames);
builder.Services.AddCategoriesModule();
builder.Services.AddWebApiAdapters();
builder.Services.AddAuctionImageConversion();
builder.Services.AddMongoDbImageDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddRabbitMq(builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>());
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddDapperAuctionhouse(builder.Configuration.GetSection("MSSql").Get<MsSqlConnectionSettings>());
builder.Services.AddQuartzTimeTaskServiceAuctionEndScheduler(builder.Configuration.GetSection("TimeTaskService").Get<TimeTaskServiceSettings>());
builder.Services.AddEfCoreReadModelNotifications(builder.Configuration.GetSection("EfCoreReadModelNotificatitons").Get<EfCoreReadModelNotificaitonsOptions>());
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
builder.Services.AddCommonWebApi(jwtConfig);
builder.Services.AddSqlServerEventOutboxStorage(builder.Configuration.GetSection("EventOutboxStorage")["ConnectionString"]);
builder.Services.AddOutboxProcessorService(new EventOutboxProcessorSettings
{
    MinMilisecondsDiff = 1500,
});

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
builder.Services.AddLogging(cfg => cfg.AddConsole());
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

RabbitMqInstaller.InitializeEventSubscriptions(app.Services, modules);
CommonInstaller.InitAttributeStrategies(moduleNames);
XmlCategoryTreeStoreInstaller.Init(app.Services);

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
