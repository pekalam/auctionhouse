using System;
using Common.Application.Queries;
using MediatR;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace ReadModel.Core
{
    public abstract class QueryHandlerBase<T, R> : IRequestHandler<T, R> where T : IQuery<R>
    {
        public Task<R> Handle(T request, CancellationToken cancellationToken)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new Collection<ValidationResult>();
            if (Validator.TryValidateObject(request, validationContext, validationResults))
            {
                return HandleQuery(request, cancellationToken);
            }
            else
            {
                //                _logger.LogDebug("Invalid command");
                //                foreach (var result in validationResults)
                //                {
                //                    _logger.LogDebug(
                //                        $"validation error membernames: {result.MemberNames.Aggregate((s, s1) => s + s1)} message: {result.ErrorMessage}");
                //                }

                throw new ArgumentException("Invalid query");
            }
        }

        protected abstract Task<R> HandleQuery(T request, CancellationToken cancellationToken);
    }
}
