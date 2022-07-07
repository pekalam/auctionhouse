using Common.Application.Commands;
using Common.Application.Queries;
using System.Reflection;

namespace Common.Application.Mediator
{
    public static class AttributeStrategies
    {
        internal static Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>> PreHandleCommandAttributeStrategies = null!;
        internal static Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>> PostHandleCommandAttributeStrategies = null!;

        public static void LoadCommandAttributeStrategies(params Assembly[] commandAssemblies)
        {
            PreHandleCommandAttributeStrategies = new Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>>();
            PostHandleCommandAttributeStrategies = new Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>>();
            var commandAttributes = commandAssemblies.SelectMany(a => a.GetTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(ICommand)))
                .Where(type => type.GetCustomAttributes(typeof(ICommandAttribute), false).Length > 0)
                .Select(type => new
                {
                    CommandType = type,
                    CommandAttributes = (ICommandAttribute[])type.GetCustomAttributes(typeof(ICommandAttribute), false)
                })
                .ToArray();

            foreach (var commandAttribute in commandAttributes)
            {
                PreHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PreHandleAttributeStrategy)
                        .ToList();
                PostHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PostHandleAttributeStrategy)
                        .ToList();
            }
        }


        internal static Dictionary<Type, List<Action<IImplProvider, IQuery>>> PreHandleQueryAttributeStrategies = null!;
        internal static Dictionary<Type, List<Action<IImplProvider, IQuery>>> PostHandleQueryAttributeStrategies = null!;

        public static void LoadQueryAttributeStrategies(params Assembly[] commandAssemblies)
        {
            PreHandleQueryAttributeStrategies = new();
            PostHandleQueryAttributeStrategies = new();
            var queryAttributes = commandAssemblies.SelectMany(a => a.GetTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(IQuery)))
                .Where(type => type.GetCustomAttributes(typeof(IQueryAttribute), false).Length > 0)
                .Select(type => new
                {
                    CommandType = type,
                    CommandAttributes = (IQueryAttribute[])type.GetCustomAttributes(typeof(IQueryAttribute), false)
                })
                .ToArray();

            foreach (var queryAttribute in queryAttributes)
            {
                PreHandleQueryAttributeStrategies[queryAttribute.CommandType] =
                    queryAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.AttributeStrategy)
                        .ToList();
            }
        }
    }
}