using Infrastructure.Bootstraper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ConfigCat.Client;
using Core.Common.RequestStatusService;
using Core.Query.ReadModel;
using Infrastructure.Auth;
using Infrastructure.Repositories.AuctionImage;
using Infrastructure.Repositories.EventStore;
using Infrastructure.Services;
using Infrastructure.Services.EventBus;
using Infrastructure.Services.SchedulerService;
using Microsoft.FeatureManagement;
using Web.Adapters;
using Web.Adapters.EventSignaling;
using Web.Auth;
using Web.Exceptions;
using Web.FeatureFlags;
using IUserIdProvider = Microsoft.AspNetCore.SignalR.IUserIdProvider;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        protected virtual void ConfigureJWT(IServiceCollection services)
        {
            var jwtConfig = Configuration.GetSection("JWT")
                .Get<JwtSettings>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtConfig.ConfigureJwt);
        }

        protected virtual void ConfigureFeatureFlagServices(IServiceCollection services)
        {
            if (!Configuration.GetValue<bool>("FeatureFlags")) {return;}

            var clientConfiguration = new LazyLoadConfiguration
            {
                ApiKey = Configuration.GetSection("ConfigCat")["ApiKey"],
                CacheTimeToLiveSeconds = Configuration.GetSection("ConfigCat").GetValue<uint>("CacheTimeToLiveSeconds")
            };

            services.AddFeatureManagement(new ConfigCatConfiguration(new ConfigCatClient(clientConfiguration), null));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtSettings>(Configuration.GetSection("JWT"));
            var eventStoreSettings = Configuration.GetSection("EventStore")
                .Get<EventStoreConnectionSettings>();
            var rabbitMqSettings = Configuration.GetSection("MQ")
                .Get<RabbitMqSettings>();
            var mongoDbSettings = Configuration.GetSection("Mongo")
                .Get<MongoDbSettings>();
            var imageDbSettings = Configuration.GetSection("ImageDb")
                .Get<ImageDbSettings>();
            var timeTaskServiceSettings = Configuration.GetSection("TimeTaskService")
                .Get<TimeTaskServiceSettings>();
            var userAuthDbSettings = Configuration.GetSection("UserAuthDb")
                .Get<UserAuthDbContextOptions>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithOrigins("http://localhost:4200");
                    	//.AllowAnyOrigin();
                });
            });
            services.AddHttpContextAccessor();
            services.AddSingleton<JwtService>();
            services.AddScoped<IRequestStatusService, RequestStatusService>();
            services.AddAutoMapper(typeof(Startup).Assembly);

            DefaultDIBootstraper.Common.Configure(services);
            DefaultDIBootstraper.Command.Configure<UserIdentityService, AuctionCreateSessionService>(
                services,
                eventStoreSettings, rabbitMqSettings, timeTaskServiceSettings, imageDbSettings, userAuthDbSettings,
                new CategoryNameServiceSettings()
                {
                    CategoriesFilePath = "./_Categories-xml-data/categories.xml",
                    SchemaFilePath = "./_Categories-xml-data/categories.xsd"
                }
            );
            DefaultDIBootstraper.Query.Configure(services, mongoDbSettings, new CategoryNameServiceSettings()
            {
                CategoriesFilePath = "./_Categories-xml-data/categories.xml",
                SchemaFilePath = "./_Categories-xml-data/categories.xsd"
            }, imageDbSettings, rabbitMqSettings);
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, UserIdProvider>();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.IsEssential = true;
            });
            services.AddMvc();
            services.AddAuthentication()
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.Scheme,
                    null);

            ConfigureFeatureFlagServices(services);
            ConfigureJWT(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            DefaultDIBootstraper.Common.Start(serviceProvider);

            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<ExceptionHandlingMiddleware>();


            app.UseAuthentication();
            app.UseSignalR(builder => { builder.MapHub<ApplicationHub>("/app"); });
            app.UseSession();
            app.UseMvc();
        }
    }
}