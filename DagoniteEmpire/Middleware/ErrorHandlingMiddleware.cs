using DagoniteEmpire.Exceptions;
using DagoniteEmpire.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

namespace DagoniteEmpire.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManagerl;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, IJSRuntime jsRuntime, NavigationManager navigationManagerl)
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
            catch(WarningException ex) 
            {
                await _jsRuntime.ToastrWarning(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = 500;
                context.Response.ContentType = Text.Plain;
                //await context.Response.WriteAsync($"Error: {ex.Message}");
                await context.Response.WriteAsync("Error:" + ex.Message);
                await _jsRuntime.ToastrError("Error" + ex.Message);
            }
        }
    }
}
