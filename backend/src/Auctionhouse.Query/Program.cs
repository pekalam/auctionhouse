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
using Adapter.SqlServer.EventOutbox;
using Common.Application.Events;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddCommon(typeof(ReadModelInstaller).Assembly);
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
builder.Services.AddReadModel(mongoDbSettings, rabbitMqSettings);
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddCategoriesModule();


builder.Services.AddSerilogLogging(builder.Configuration, "Query");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddControllers();
var jwtConfig = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddCommonJwtAuth(jwtConfig, authBuilder);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMongoDbImageDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddEfCoreReadModelNotifications(builder.Configuration.GetSection("EfCoreReadModelNotificatitons").Get<EfCoreReadModelNotificaitonsOptions>());
//TODO remove unnecessar query dependecies
builder.Services.AddSqlServerEventOutboxStorage(builder.Configuration.GetSection("EventOutboxStorage")["ConnectionString"]);
builder.Services.AddSingleton(new EventBusSettings
{
    MaxRedelivery = Convert.ToInt32(builder.Configuration.GetSection(nameof(EventBusSettings))[nameof(EventBusSettings.MaxRedelivery)])
});
builder.Services.AddEventRedeliveryProcessorService();

builder.Services.AddInstrumentationDecorators();

CommonInstaller.AddTracing(b => {
    b.AddAspNetCoreInstrumentation();
});

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
var tracing = CommonInstaller.CreateModuleTracing("Query");

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
tracing.Dispose();