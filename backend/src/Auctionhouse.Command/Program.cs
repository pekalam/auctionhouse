using Adapter.AuctionImageConversion;
using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.MongoDb;
using Adapter.MongoDb.AuctionImage;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using AuctionBids.Application;
using Auctionhouse.Command.Adapters;
using Auctionhouse.Command.Auth;
using Auctions.Application;
using Categories.Domain;
using Common.Application;
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
builder.Services.AddAuctionsModule();
builder.Services.AddCategoriesModule();
builder.Services.AddWebApiAdapters();
builder.Services.AddAuctionImageConversion();
builder.Services.AddMongoDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddRabbitMq(builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>());
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddDapperAuctionhouse(builder.Configuration.GetSection("MSSql").Get<MsSqlConnectionSettings>());
builder.Services.AddQuartzTimeTaskServiceAuctionEndScheduler(builder.Configuration.GetSection("TimeTaskService").Get<TimeTaskServiceSettings>());

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
