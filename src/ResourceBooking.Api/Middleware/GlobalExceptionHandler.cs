using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Api.Middleware;

/// <summary>
/// Translates exceptions that escape MediatR handlers into ProblemDetails
/// responses with an appropriate status code, so controllers don't need
/// try/catch blocks around every _sender.Send call.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapToResponse(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message,
        };

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            problemDetails.Extensions["errors"] = errors;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapToResponse(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
        BookingConflictException => (StatusCodes.Status409Conflict, "Booking Conflict"),
        ResourceInactiveException => (StatusCodes.Status409Conflict, "Resource Inactive"),
        DuplicateEmailException => (StatusCodes.Status409Conflict, "Email Already Registered"),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
        DomainException => (StatusCodes.Status400BadRequest, "Bad Request"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
    };
}
