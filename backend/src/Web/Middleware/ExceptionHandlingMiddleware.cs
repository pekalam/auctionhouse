using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Exceptions.Common;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Http;

namespace Web.Middleware
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
                case UserNotSignedInException e:
                    apiException = new ApiException(HttpStatusCode.Forbidden, "user not signed in", e);
                    break;
                default:
                    apiException = new ApiException(HttpStatusCode.InternalServerError, "error", ex);
                    break;
            }
            HandleException(apiException, context);
        }
    }
}
