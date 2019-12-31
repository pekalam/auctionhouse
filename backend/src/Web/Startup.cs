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
using Core.Common.RequestStatusService;
using Core.Query.ReadModel;
using Infrastructure.Auth;
using Infrastructure.Repositories.AuctionImage;
using Infrastructure.Repositories.SQLServer;
using Infrastructure.Services;
using Infrastructure.Services.EventBus;
using Infrastructure.Services.SchedulerService;
using Swashbuckle.AspNetCore.Swagger;
using Web.Adapters;
using Web.Adapters.EventSignaling;
using Web.Auth;
using Web.Exceptions;
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
            var jwtConfig = Configuration.GetSection("JWT")
                .Get<JwtSettings>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtConfig.ConfigureJwt);
        }

        private void AddOptions<T>(IServiceCollection serviceCollection, string sectionName) where T : class
        {
            var settings = Configuration.GetSection(sectionName)
                .Get<T>();
            serviceCollection.AddSingleton(settings);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtSettings>(Configuration.GetSection("JWT"));
            var sqlServerSettings = Configuration.GetSection("SQLServer")
                .Get<MsSqlConnectionSettings>();
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
            AddOptions<ResetLinkSenderServiceSettings>(services, "ResetLinkService");

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

            var categoriesXmlDirLocation = Environment.GetEnvironmentVariable("categories_xml_data");

            DefaultDIBootstraper.Common.Configure(services);
            DefaultDIBootstraper.Command.Configure<UserIdentityService, AuctionCreateSessionService, ResetLinkSenderService>(
                services,
                sqlServerSettings, rabbitMqSettings, timeTaskServiceSettings, imageDbSettings, userAuthDbSettings,
                new CategoryNameServiceSettings()
                {
                    CategoriesFilePath = $"{categoriesXmlDirLocation}/categories.xml",
                    SchemaFilePath = $"{categoriesXmlDirLocation}/categories.xsd"
                }
            );
            DefaultDIBootstraper.Query.Configure(services, mongoDbSettings, new CategoryNameServiceSettings()
            {
                CategoriesFilePath = $"{categoriesXmlDirLocation}/categories.xml",
                SchemaFilePath = $"{categoriesXmlDirLocation}/categories.xsd"
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

            ConfigureJWT(services);


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info(){ Title = "Auctionhouse API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            DefaultDIBootstraper.Common.Start(serviceProvider);

            app.UseCors();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auctionhouse API V1");
            });

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