using DagoniteEmpire.Exceptions;
using DagoniteEmpire.Helper;
using Microsoft.JSInterop;
using static System.Net.Mime.MediaTypeNames;

namespace DagoniteEmpire.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IJSRuntime _jsRuntime;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, IJSRuntime jsRuntime)
        {
            _logger = logger;
            _jsRuntime = jsRuntime;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (RepositoryErrorException ex)
            {
                _logger.LogError(ex, ex.Message);
                await TryNotifyAsync(() => _jsRuntime.ToastrError(ex.Message));
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = Text.Plain;
                    await context.Response.WriteAsync(ex.Message);
                }
            }
            catch (WarningException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                await TryNotifyAsync(() => _jsRuntime.ToastrWarning(ex.Message));
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = Text.Plain;
                    await context.Response.WriteAsync(ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = Text.Plain;
                    await context.Response.WriteAsync("Error:" + ex.Message);
                }
                await TryNotifyAsync(() => _jsRuntime.ToastrError("Error" + ex.Message));
            }
        }

        /// <summary>
        /// Toastr needs an active Blazor circuit. Skip JS during static HTTP endpoints
        /// (e.g. /Account/DemoBaron) so the real exception is not replaced by a JS-interop failure.
        /// </summary>
        private async Task TryNotifyAsync(Func<ValueTask> notify)
        {
            try
            {
                await notify();
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("JavaScript interop", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("statically rendered", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug(ex, "Skipped toastr notification outside an interactive Blazor circuit.");
            }
            catch (JSException ex)
            {
                _logger.LogDebug(ex, "Skipped toastr notification; JS runtime unavailable.");
            }
        }
    }
}
