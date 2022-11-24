using SilverSoft.Logging;
namespace SilverSoft.Middlewares
{
    /// <summary>
    /// 异常处理中间件
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            this.next = next;
            this._logger = logger;
        }
        public async Task<Task> Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json;charset=utf-8";
                }
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new { code = -20210415, msg = ex.Message ?? "发生未知错误" }));
            }

            return Task.CompletedTask;
        }
    }
}