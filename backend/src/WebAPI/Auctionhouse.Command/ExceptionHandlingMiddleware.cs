using Common.Application.Commands;
using Core.Common.Domain.Users;
using Core.DomainFramework;
using System.Net;
using Users.Application.Exceptions;

namespace Auctionhouse.Command
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InfrastructureException ex)
            {
                HandleException(ex, context);
            }
            catch (DomainException ex)
            {
                HandleException(ex, context);
            }
            catch (Exception ex)
            {
                HandleException(ex, context);
            }
        }

        private void HandleException(Exception ex, HttpContext context) //TODO codes
        {
            switch (ex)
            {
                case InvalidCommandDataException e:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case UserNotFoundException e:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case InvalidPasswordException e:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case UnauthorizedAccessException e:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                default:
                    _logger.LogWarning(ex, $"Exception not handled in {nameof(ExceptionHandlingMiddleware)}");
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
            }
        }

        private void HandleException(InfrastructureException ex, HttpContext context)
        {
            _logger.LogWarning(ex, $"Infrastructure exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.WriteAsync("Internal server error");
        }

        private void HandleException(DomainException ex, HttpContext context)
        {
            switch (ex)
            {
                case InvalidUsernameException e:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    _logger.LogWarning(ex, $"{nameof(DomainException)} not handled in {nameof(ExceptionHandlingMiddleware)}");
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
            }
        }
    }
}
