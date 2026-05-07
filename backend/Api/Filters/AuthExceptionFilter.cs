using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ToolLendingPlatform.Domain.Exceptions;
using ToolLendingPlatform.Api.Dtos;

namespace ToolLendingPlatform.Api.Filters
{
    /// <summary>
    /// Exception filter for authentication-related exceptions.
    /// Rule #12: Maps domain/application exceptions to HTTP responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // Rule #4: DuplicateUsernameException → 400 Bad Request
            if (exception is DuplicateUsernameException dupEx)
            {
                context.Result = new BadRequestObjectResult(new ErrorResponseDto
                {
                    Error = "ConflictError",
                    Details = new { message = "Username already taken" }
                });
                context.ExceptionHandled = true;
                return;
            }

            // Rule #5: AuthenticationException → 401 Unauthorized
            if (exception is AuthenticationException authEx)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponseDto
                {
                    Error = "AuthenticationError",
                    Details = new { message = authEx.Message }
                });
                context.ExceptionHandled = true;
                return;
            }

            // Rule #3: InvalidPasswordException, InvalidUsernameException → 400 Bad Request
            if (exception is InvalidPasswordException or InvalidUsernameException)
            {
                context.Result = new BadRequestObjectResult(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Details = new { message = exception.Message }
                });
                context.ExceptionHandled = true;
                return;
            }

            base.OnException(context);
        }
    }
}
