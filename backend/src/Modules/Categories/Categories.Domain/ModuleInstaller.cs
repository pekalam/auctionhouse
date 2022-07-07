using Core.Common.Domain.Categories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Categories.Domain
{
    public static class ModuleInstaller
    {
        public static void AddCategoriesModule(this IServiceCollection services) 
        {
            services.AddTransient<CategoryBuilder>();
        }
    }
}
