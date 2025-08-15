using HuloToys_Service.Utilities.Lib;

namespace HuloToys_Service.Utilities.Middleware
{
    public class GlobalErrorCaptureMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalErrorCaptureMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context)
        {
            Exception? caughtException = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                caughtException = ex; // Lưu lại nguyên nhân
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            finally
            {
                if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
                {
                    string reason = caughtException != null
                        ? $"{caughtException.GetType().Name}: {caughtException.Message}\nStackTrace:\n{caughtException.StackTrace}"
                        : "500 Internal Server Error (No exception captured)";

                     SendToTelegramAsync(context, reason);
                }
            }
        }

        private void SendToTelegramAsync(HttpContext context, string reason)
        {
            try
            {
                var message = $"🔥 500 Error Detected\n" +
                              $"Path: {context.Request.Path}\n" +
                              $"Query: {context.Request.QueryString}\n" +
                              $"Reason: {reason}\n" +
                              $"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                LogHelper.InsertLogTelegram(message);
            }
            catch
            {
                // tránh crash nếu Telegram lỗi
            }
        }
    }

}
