using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Web.Assistant;
using System.Text.Json;

namespace Portfolio.Web.Endpoints;

public static class AssistantEndpoints
{
    private const long MaximumRequestBytes = 12 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapPortfolioAssistantEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/assistant/chat",
                HandleChatAsync)
            .DisableAntiforgery()
            .RequireRateLimiting("portfolio-assistant")
            .WithName("PortfolioAssistantChat");

        return endpoints;
    }

    private static async Task<Results<
        Ok<AssistantChatResponse>,
        BadRequest<object>,
        StatusCodeHttpResult,
        JsonHttpResult<object>>> HandleChatAsync(
        HttpContext httpContext,
        IPortfolioAssistantService assistant,
        CancellationToken cancellationToken)
    {
        if (httpContext.Request.ContentLength is > MaximumRequestBytes)
        {
            return TypedResults.Json<object>(
                new { error = "request_too_large" },
                statusCode: StatusCodes.Status413PayloadTooLarge);
        }

        if (!httpContext.Request.HasJsonContentType())
        {
            return TypedResults.Json<object>(
                new { error = "json_required" },
                statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        if (!IsSameOriginRequest(httpContext.Request))
        {
            return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
        }

        var (request, readError) = await ReadRequestAsync(
            httpContext.Request,
            cancellationToken);
        if (request is null)
        {
            return TypedResults.Json<object>(
                new { error = readError },
                statusCode: readError == "request_too_large"
                    ? StatusCodes.Status413PayloadTooLarge
                    : StatusCodes.Status400BadRequest);
        }

        if (!AssistantRequestValidator.TryValidate(request, out var validated, out var validationError)
            || validated is null)
        {
            return TypedResults.BadRequest<object>(new { error = validationError });
        }

        try
        {
            var result = await assistant.AskAsync(validated, cancellationToken);
            return TypedResults.Ok(new AssistantChatResponse(
                result.Answer,
                result.Sources,
                result.Confidence));
        }
        catch (PortfolioAssistantUnavailableException)
        {
            return TypedResults.Json<object>(
                new { error = "assistant_unavailable" },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<(AssistantChatRequest? Request, string Error)> ReadRequestAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[MaximumRequestBytes + 1];
        var totalBytes = 0;

        while (totalBytes < buffer.Length)
        {
            var bytesRead = await request.Body.ReadAsync(
                buffer.AsMemory(totalBytes, buffer.Length - totalBytes),
                cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;
        }

        if (totalBytes > MaximumRequestBytes)
        {
            return (null, "request_too_large");
        }

        if (totalBytes == 0)
        {
            return (null, "invalid_request");
        }

        try
        {
            return (
                JsonSerializer.Deserialize<AssistantChatRequest>(
                    buffer.AsSpan(0, totalBytes),
                    JsonOptions),
                "invalid_request");
        }
        catch (JsonException)
        {
            return (null, "invalid_request");
        }
    }

    private static bool IsSameOriginRequest(HttpRequest request)
    {
        var fetchSite = request.Headers["Sec-Fetch-Site"].ToString();
        if (!string.IsNullOrWhiteSpace(fetchSite)
            && !string.Equals(fetchSite, "same-origin", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var originHeader = request.Headers.Origin.ToString();
        if (!string.IsNullOrWhiteSpace(originHeader))
        {
            return IsExpectedOrigin(originHeader, request);
        }

        var refererHeader = request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(refererHeader))
        {
            return IsExpectedOrigin(refererHeader, request);
        }

        return string.Equals(fetchSite, "same-origin", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExpectedOrigin(string value, HttpRequest request) =>
        Uri.TryCreate(value, UriKind.Absolute, out var origin)
            && string.Equals(origin.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase)
            && NormalizePort(origin) == NormalizePort(request);

    private static int NormalizePort(Uri origin) =>
        origin.IsDefaultPort
            ? origin.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80
            : origin.Port;

    private static int NormalizePort(HttpRequest request)
    {
        if (request.Host.Port.HasValue)
        {
            return request.Host.Port.Value;
        }

        var forwardedScheme = request.Headers["X-Forwarded-Proto"]
            .ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        var scheme = string.IsNullOrWhiteSpace(forwardedScheme)
            ? request.Scheme
            : forwardedScheme;

        return scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
    }
}
