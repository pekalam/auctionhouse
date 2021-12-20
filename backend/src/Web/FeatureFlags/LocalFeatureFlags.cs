using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Web.FeatureFlags
{
    public static class LocalFeatureFlags
    {
        public static void AddLocalFeatureFlags(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddFeatureManagement(configuration.GetSection("LocalFeatureFlags"));
        }
    }
}
