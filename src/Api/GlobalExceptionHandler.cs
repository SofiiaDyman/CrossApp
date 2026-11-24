using Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problems, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title, detail) = ex switch
        {
            DomainException => (StatusCodes.Status409Conflict,
                                "Порушено бізнес-правило", ex.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest,
                                "Некоректний запит", "Тіло запиту має некоректний формат."),
            ArgumentException => (StatusCodes.Status400BadRequest,
                                "Некоректні дані", ex.Message),
            _ => (StatusCodes.Status500InternalServerError,
                                "Внутрішня помилка сервера", (string?)null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Необроблений виняток");

        ctx.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }
}
