using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Query;
using MediatR;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Core.Query.Mediator
{
    public class QueryMediator
    {
        private const string QueryAssemblyName = "Core.Query";

        private IMediator _mediator;
        private IImplProvider _implProvider;
        private readonly ILogger<QueryMediator> _logger;
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

        public QueryMediator(IMediator mediator, IImplProvider implProvider, ILogger<QueryMediator> logger)
        {
            _mediator = mediator;
            _implProvider = implProvider;
            _logger = logger;
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

            _logger.LogTrace("Handling query {name}", query.GetType().Name);
            try
            {
                return await _mediator.Send(query);
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Query {name} exception", query.GetType().Name);
                throw e;
            }
        }
    }
}
