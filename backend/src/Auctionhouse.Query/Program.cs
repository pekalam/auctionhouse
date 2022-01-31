using Adapter.EfCore.ReadModelNotifications;
using Adapter.MongoDb;
using Adapter.MongoDb.AuctionImage;
using Auctionhouse.Query;
using Auctionhouse.Query.Adapters;
using Categories.Domain;
using Common.Application;
using Common.WebAPI;
using Common.WebAPI.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Trace;
using RabbitMq.EventBus;
using ReadModel.Core;
using ReadModel.Core.Model;
using ReadModel.Core.Services;
using Serilog;
using XmlCategoryTreeStore;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

//MODULES
builder.Services.AddCommonQueryDependencies(typeof(ReadModelInstaller).Assembly);
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
builder.Services.AddReadModel(mongoDbSettings);
builder.Services.AddCategoriesModule();

//ADAPTERS
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
builder.Services.AddRabbitMq(rabbitMqSettings, eventConsumerAssemblies: new[] { typeof(ReadModelInstaller).Assembly });
builder.Services.AddXmlCategoryTreeStore(builder.Configuration.GetSection("XmlCategoryTreeStore").Get<XmlCategoryNameStoreSettings>());
builder.Services.AddMongoDbImageDb(builder.Configuration.GetSection("ImageDb").Get<ImageDbSettings>());
builder.Services.AddEfCoreReadModelNotifications(builder.Configuration.GetSection("EfCoreReadModelNotificatitons").Get<EfCoreReadModelNotificaitonsOptions>());
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
    b.AddAspNetCoreInstrumentation();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCacheServices(builder.Configuration);

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

XmlCategoryTreeStoreInstaller.Init(app.Services);
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

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapHub<ApplicationHub>("/app");
app.MapControllers();

app.Run();
tracing.Dispose();