using System.Net;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Exceptions.Common;
using Core.Command.SignIn;
using Core.Command.SignUp;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace Web.Exceptions
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                HandleException(ex, context);
            }
            catch (CommandException ex)
            {
                HandleException(ex, context);
            }
            catch (DomainException ex)
            {
                HandleException(ex, context);
            }
        }

        private void HandleException(ApiException ex, HttpContext context)
        {
            context.Response.StatusCode = (int) ex.StatusCode;
        }

        private void HandleException(CommandException ex, HttpContext context)
        {
            ApiException apiException;
            switch (ex)
            {
                case UsernameConflictException e:
                    apiException = new ApiException(HttpStatusCode.Conflict, "User exists", e);
                    break;
                case UserNotFoundException e:
                    apiException = new ApiException(HttpStatusCode.Unauthorized, "Invalid credentials", e);
                    break;
                case InvalidPasswordException e:
                    apiException = new ApiException(HttpStatusCode.Unauthorized, "Invalid credentials", e);
                    break;
                case UserNotSignedInException e:
                    apiException = new ApiException(HttpStatusCode.Forbidden, "user not signed in", e);
                    break;
                default:
                    apiException = new ApiException(HttpStatusCode.InternalServerError, "error", ex);
                    break;
            }
            HandleException(apiException, context);
        }

        private void HandleException(DomainException ex, HttpContext context)
        {
            ApiException apiException;
            switch (ex)
            {
                case InvalidUsernameException e:
                    apiException = new ApiException(HttpStatusCode.BadRequest, "Invalid username", e);
                    break;
                default:
                    apiException = new ApiException(HttpStatusCode.BadRequest, "error", ex);
                    break;
            }
            HandleException(apiException, context);
        }

    }
}
