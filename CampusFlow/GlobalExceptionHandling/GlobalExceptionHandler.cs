using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CampusFlow.GlobalExceptionHandling
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);

            var (statusCode, title, detail) = exception switch
            {
                BadHttpRequestException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    "The request could not be processed."
                ),

                DuplicateEmailException => (
                    StatusCodes.Status409Conflict,
                    "Account already exists",
                    exception.Message
                ),

                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    "One or more request values are invalid."
                ),

                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "The requested resource was not found."
                ),

                UnauthorizedAccessException => (
                    StatusCodes.Status403Forbidden,
                    "Access denied",
                    "You do not have permission to perform this action."
                ),

                InvalidOperationException => (
                    StatusCodes.Status409Conflict,
                    "Operation could not be completed",
                    "The operation conflicts with the current resource state."
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred. Please try again later."
                )
            };
            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
