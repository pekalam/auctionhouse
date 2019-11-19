using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Common.Attributes;
using MediatR;

namespace Core.Common.Query
{
    public class QueryMediator
    {
        private const string QueryAssemblyName = "Core.Query";

        private IMediator _mediator;
        private IImplProvider _implProvider;
        private static Dictionary<Type, IEnumerable<Action<IImplProvider, IQuery>>> _queryAttributeStrategies;

        static QueryMediator()
        {
            LoadQueryAttributeStrategies(QueryAssemblyName);
        }

        internal static void LoadQueryAttributeStrategies(string commandsAssemblyName)
        {
            _queryAttributeStrategies = new Dictionary<Type, IEnumerable<Action<IImplProvider, IQuery>>>();
            var queryAttributes = Assembly.Load(commandsAssemblyName)
                .GetTypes()
                .Where(type => type.GetInterfaces().Contains(typeof(IQuery)))
                .Where(type => type.GetCustomAttributes(typeof(IQueryAttribute), false).Length > 0)
                .Select(type => new
                {
                    QueryType = type,
                    QueryAttributes = (IQueryAttribute[])type.GetCustomAttributes(typeof(IQueryAttribute), false)
                })
                .ToArray();

            foreach (var commandAttribute in queryAttributes)
            {
                _queryAttributeStrategies[commandAttribute.QueryType] =
                    commandAttribute.QueryAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.AttributeStrategy)
                        .AsEnumerable();
            }
        }

        public QueryMediator(IMediator mediator, IImplProvider implProvider)
        {
            _mediator = mediator;
            _implProvider = implProvider;
        }

        public async Task<T> Send<T>(IQuery<T> query)
        {
            if (_queryAttributeStrategies.ContainsKey(query.GetType()))
            {
                foreach (var strategy in _queryAttributeStrategies[query.GetType()])
                {
                    strategy.Invoke(_implProvider, query);
                }
            }

            return await _mediator.Send(query);
        }
    }
}
