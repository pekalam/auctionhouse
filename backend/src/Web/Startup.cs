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
using Core.Common.EventSignalingService;
using Core.Query.ReadModel;
using Infrastructure.Adapters.Repositories.AuctionImage;
using Infrastructure.Adapters.Repositories.EventStore;
using Infrastructure.Adapters.Services;
using Infrastructure.Adapters.Services.EventBus;
using Infrastructure.Adapters.Services.SchedulerService;
using Web.Adapters;
using Web.Adapters.EventSignaling;
using Web.Middleware;
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

        private void ConfigureJWT(IServiceCollection services)
        {
            var jwtConfig = Configuration.GetSection("JWT").Get<JwtSettings>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtConfig.ConfigureJwt);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtSettings>(Configuration.GetSection("JWT"));
            var eventStoreSettings = Configuration.GetSection("EventStore").Get<EventStoreConnectionSettings>();
            var rabbitMqSettings = Configuration.GetSection("MQ").Get<RabbitMqSettings>();
            var mongoDbSettings = Configuration.GetSection("Mongo").Get<MongoDbSettings>();
            var imageDbSettings = Configuration.GetSection("ImageDb").Get<ImageDbSettings>();
            var timeTaskServiceSettings = Configuration.GetSection("TimeTaskService").Get<TimeTaskServiceSettings>();

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
            services.AddScoped<IEventSignalingService, EventSignalingService>();

            MicrosoftDIBootstraper.Bootstrap<UserIdentityService, AuctionCreateSessionService>(services, 
                eventStoreSettings, rabbitMqSettings, 
                mongoDbSettings, timeTaskServiceSettings, new CategoryNameServiceSettings()
                {
                    CategoriesFilePath = "./_data/categories.xml"
                }, imageDbSettings);
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

            ConfigureJWT(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            MicrosoftDIBootstraper.Init(serviceProvider);

            app.UseCors();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseSignalR(builder =>
            {
                builder.MapHub<ApplicationHub>("/app");
            });
            app.UseSession();
            app.UseMvc();
        }
    }
}