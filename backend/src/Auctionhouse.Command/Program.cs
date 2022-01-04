using Auctions.Application;
using AuctionBids.Application;
using RabbitMq.EventBus;
using XmlCategoryTreeStore;
using Dapper.AuctionhouseDatabase;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using QuartzTimeTaskService.AuctionEndScheduler;
using System.Reflection;
using Common.Application;
using Adapter.MongoDb;
using Adapter.MongoDb.AuctionImage;
using Adapter.AuctionImageConversion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCommon();
builder.Services.AddAuctionBidsModule();
builder.Services.AddAuctionsModule();
builder.Services.AddAuctionImageConversion();
builder.Services.AddMongoDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddRabbitMq(builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>());
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddDapperAuctionhouse(builder.Configuration.GetSection("MSSql").Get<MsSqlConnectionSettings>());
builder.Services.AddQuartzTimeTaskServiceAuctionEndScheduler(builder.Configuration.GetSection("TimeTaskService").Get<TimeTaskServiceSettings>());

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

RabbitMqInstaller.InitializeEventSubscriptions(app.Services, 
    Assembly.Load("Auctions.Application"), 
    Assembly.Load("UserPayments.Application"), 
    Assembly.Load("AuctionBids.Application"));


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
