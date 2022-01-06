using Common.Application;
using Common.WebAPI.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

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
    }
}