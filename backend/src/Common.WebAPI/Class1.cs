using Common.Application;
using Common.WebAPI.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Common.WebAPI
{
    public static class CommonWebApiInstaller
    {
        public static void AddCommonWebApi(this IServiceCollection services, JwtSettings jwtConfig)
        {
            services.AddSingleton(jwtConfig);
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtConfig.ConfigureJwt);
            services.AddTransient<JwtService>();
            services.AddTransient<IUserIdentityService, UserIdentityService>();
        }

        public static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration, string appName)
        {
            var config = new LoggerConfiguration();
            Log.Logger = config
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console(outputTemplate: "[{SourceContext}] [{Level:u3}] {Message:lj}{Properties:j}{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}{NewLine}{Exception}")
                .Enrich.WithProperty("AppName", appName)
                .CreateLogger();

            services.AddLogging(b => b.AddSerilog(dispose: true));
        }
    }
}