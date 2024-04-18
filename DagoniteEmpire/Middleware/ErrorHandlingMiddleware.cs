using DagoniteEmpire.Exceptions;
using DagoniteEmpire.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            catch(RepositoryErrorException ex)
            {
                await _jsRuntime.ToastrError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Uknown error");
                await _jsRuntime.ToastrError("Uknown error");
            }
        }
    }
}
