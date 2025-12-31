using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GestiParc.Api.Logging;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST",
        "PUT",
        "PATCH",
        "DELETE"
    };

    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(ILogger<AuditActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        var method = http.Request.Method;
        if (!MutatingMethods.Contains(method))
        {
            await next();
            return;
        }

        // Auth uniquement : pas d'audit sur /authentifier
        var path = http.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/utilisateur/authentifier", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        if (!(http.User?.Identity?.IsAuthenticated ?? false))
        {
            await next();
            return;
        }

        var controller = (context.RouteData.Values.TryGetValue("controller", out var c) ? c?.ToString() : null) ?? "?";
        var entityType = controller;
        var operation = method.ToUpperInvariant() switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => method.ToUpperInvariant()
        };

        string? entityId = TryGetEntityIdFromRoute(context);

        try
        {
            var executed = await next();

            var statusCode = TryGetStatusCode(executed.Result) ?? http.Response.StatusCode;

            // Sur les créations, l'id peut être uniquement dans la réponse.
            entityId ??= TryGetEntityIdFromResult(executed.Result);

            var outcome = statusCode >= 200 && statusCode < 300 ? "Success" : "Fail";

            _logger.LogInformation(
                "[AUDIT] {Operation} {EntityType} {EntityId} => {StatusCode} ({Outcome})",
                operation,
                entityType,
                entityId ?? "-",
                statusCode,
                outcome);
        }
        catch
        {
            _logger.LogError("[AUDIT] {Operation} {EntityType} {EntityId} => EXCEPTION", operation, entityType, entityId ?? "-");
            throw;
        }
    }

    private static string? TryGetEntityIdFromRoute(ActionExecutingContext context)
    {
        foreach (var (key, value) in context.RouteData.Values)
        {
            if (string.Equals(key, "controller", StringComparison.OrdinalIgnoreCase))
                continue;
            if (string.Equals(key, "action", StringComparison.OrdinalIgnoreCase))
                continue;

            var str = value?.ToString();
            if (!string.IsNullOrWhiteSpace(str))
                return str;
        }

        return null;
    }

    private static int? TryGetStatusCode(IActionResult? result)
    {
        return result switch
        {
            ObjectResult o => o.StatusCode,
            StatusCodeResult s => s.StatusCode,
            _ => null
        };
    }

    private static string? TryGetEntityIdFromResult(IActionResult? result)
    {
        if (result is not ObjectResult { Value: not null } objectResult)
            return null;

        var value = objectResult.Value;
        var type = value.GetType();

        static string? TryGetProp(object instance, Type t, string propName)
        {
            var p = t.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            var v = p?.GetValue(instance);
            var s = v?.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        return TryGetProp(value, type, "Id")
               ?? TryGetProp(value, type, "Idrh")
               ?? TryGetProp(value, type, "IDRH");
    }
}
