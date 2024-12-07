
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Serilog.Context;

public class LoggingActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        LogContext.PushProperty("SourceContext", "ActionFilter");
        var request = context.HttpContext.Request;

        Log.Information("****ActionFilter**** Request for {Method} {Path} from {IPAddress} with data {@Data}",
            request.Method,
            request.Path,
            context.HttpContext.Connection.RemoteIpAddress,
            context.ActionArguments);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        LogContext.PushProperty("SourceContext", "ActionFilter");
        var response = context.HttpContext.Response;

        Log.Information("****ActionFilter**** Response from {StatusCode} with data {@Data}",
            response.StatusCode,
            context.Result);
    }
}