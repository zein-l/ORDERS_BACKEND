using Microsoft.AspNetCore.Mvc;                    // ProblemDetails type
using Microsoft.AspNetCore.Mvc.Infrastructure;     // ProblemDetailsFactory
using System.Net;

namespace Orders.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProblemDetailsFactory _problemFactory;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ProblemDetailsFactory problemFactory,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _problemFactory = problemFactory;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (OperationCanceledException)
        {
            // Client aborted
            ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (InvalidOperationException ex)
        {
            await WriteProblem(ctx, StatusCodes.Status400BadRequest, "Invalid operation", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(ctx, StatusCodes.Status401Unauthorized, "Unauthorized", ex);
        }
        catch (Exception ex)
        {
            await WriteProblem(ctx, StatusCodes.Status500InternalServerError, "Unexpected error", ex);
        }
    }

    private async Task WriteProblem(HttpContext ctx, int statusCode, string title, Exception ex)
    {
        var correlationId = ctx.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var v)
            ? v?.ToString()
            : null;

        _logger.LogError(ex, "{Title}. Status={Status} CorrelationId={CorrelationId}",
            title, statusCode, correlationId);

        var pd = _problemFactory.CreateProblemDetails(ctx,
            statusCode: statusCode,
            title: title,
            detail: ex.Message);

        pd.Extensions["correlationId"] = correlationId;

        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = statusCode;

        await ctx.Response.WriteAsJsonAsync(pd);
    }
}
