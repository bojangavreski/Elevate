using System.Net;
using System.Text.Json;

namespace Elevate.API
{
    public class GlobalExceptionMiddleware(RequestDelegate next)
    {
            public async Task InvokeAsync(HttpContext httpContext)
            {
                try
                {
                    await next(httpContext);
                }
                catch (Exception ex)
                {
                    //we can log something for monitoring 
                    await HandleExceptionAsync(httpContext, ex);
                }
            }

            private static async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
            {
                var (statusCode, message) = ex switch
                {
                    OperationCanceledException =>((int)HttpStatusCode.InternalServerError, ex.Message),
                    ArgumentNullException or
                    ArgumentException or
                    InvalidOperationException => ((int)HttpStatusCode.BadRequest, ex.Message),
                    _ => ((int)HttpStatusCode.InternalServerError, "Unexpected error ocrued")
                };

                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = statusCode;

                var response = new ErrorDetails
                {
                    StatusCode = statusCode,
                    Message = message
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await httpContext.Response.WriteAsync(json);
            }

            public class ErrorDetails
            {
                public int StatusCode { get; set; }
                public required string Message { get; set; }
            }
    }
}
