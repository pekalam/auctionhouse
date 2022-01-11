using Common.Application;
using RabbitMq.EventBus;
using ReadModel.Core;
using ReadModel.Core.Model;
using XmlCategoryTreeStore;
using Categories.Domain;
using Common.WebAPI.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Common.WebAPI;
using Adapter.MongoDb;
using Adapter.MongoDb.AuctionImage;
using Adapter.EfCore.ReadModelNotifications;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCommon(typeof(ReadModelInstaller).Assembly);
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
builder.Services.AddReadModel(mongoDbSettings, rabbitMqSettings);
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddCategoriesModule();


builder.Services.AddLogging(cfg => cfg.AddConsole());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddControllers();
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
builder.Services.AddCommonWebApi(jwtConfig);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMongoDbImageDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddEfCoreReadModelNotifications(builder.Configuration.GetSection("EfCoreReadModelNotificatitons").Get<EfCoreReadModelNotificaitonsOptions>());

var allowedOrigin = builder.Configuration.GetValue<string>("CORS:AllowedOrigin");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(allowedOrigin);
    });
});


var app = builder.Build();

CommonInstaller.InitAttributeStrategies("ReadModel.Core");
ReadModelInstaller.InitSubscribers(app.Services);
XmlCategoryTreeStoreInstaller.Init(app.Services);

app.UseAuthentication();
app.UseStaticFiles();

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
