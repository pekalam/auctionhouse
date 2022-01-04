using System.Reflection;
using AutoMapper;

namespace ReadModel.Core
{
    internal class MapperConfigHolder
    {
        public static MapperConfiguration Configuration { get; }

        static MapperConfigHolder()
        {
            Configuration = new MapperConfiguration(expression =>
            {
                expression.AddMaps(Assembly.GetAssembly(typeof(MapperConfigHolder)));
            });
        }
    }
}