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

    private static async Task<IResult> HandleChatAsync(
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
            return TypedResults.Json<object>(new { error = "origin_rejected" },
                statusCode: StatusCodes.Status403Forbidden);
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

        httpContext.Response.Headers.CacheControl = "no-store";
        if (httpContext.Request.Headers.Accept.ToString() == "application/x-ndjson")
        {
            return Results.Stream(async stream =>
            {
                await WriteEventAsync(stream, new { stage = "reading" }, cancellationToken);
                try
                {
                    var reply = await RespondAsync(assistant, validated, cancellationToken);
                    await WriteEventAsync(stream, new { stage = "complete", reply }, cancellationToken);
                }
                catch (PortfolioAssistantUnavailableException)
                {
                    await WriteEventAsync(stream, new { stage = "unavailable" }, cancellationToken);
                }
            }, "application/x-ndjson");
        }

        try
        {
            return TypedResults.Ok(await RespondAsync(assistant, validated, cancellationToken));
        }
        catch (PortfolioAssistantUnavailableException)
        {
            return TypedResults.Json<object>(
                new { error = "assistant_unavailable" },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<AssistantChatResponse> RespondAsync(IPortfolioAssistantService assistant,
        ValidatedAssistantRequest request, CancellationToken cancellationToken)
    {
        var navigation = AssistantNavigationPolicy.FromQuestion(request);
        if (navigation is not null)
        {
            return new AssistantChatResponse(
                request.Language == "ar" ? "يمكنك فتح الوجهة من الرابط أدناه." : "Use the link below to open the destination.",
                [], "navigation", navigation);
        }

        var answer = await assistant.AskAsync(request, cancellationToken);
        return new AssistantChatResponse(answer.Answer, answer.Sources, answer.Confidence);
    }

    private static async Task WriteEventAsync<T>(Stream stream, T data, CancellationToken cancellationToken)
    {
        await JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken);
        await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        await stream.FlushAsync(cancellationToken);
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
