using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;
using System.Threading.Tasks;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        request.EnableBuffering(); 

        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        Log.Information("****Middleware**** Incoming request: {Method} {Path} with Body: {Body}", 
            request.Method, 
            request.Path, 
            requestBody);

        request.Body.Position = 0;

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        Log.Information("****Middleware**** Outgoing response: {StatusCode} with Body: {Body}", 
            context.Response.StatusCode, 
            responseBodyText);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
