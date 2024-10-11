using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model;
using BasicWebNovelAPI.Model.Errors;
using System.Diagnostics;
using System.Net;

namespace BasicWebNovelAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }   

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something Wrong: {ex}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                NotFoundException => HttpStatusCode.NotFound,
                BadRequestException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorDetail()
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                RequestId = context.TraceIdentifier,
                StackTrace = ex.StackTrace
            };

            await context.Response.WriteAsync(response.ToString());
        }
    }
}
