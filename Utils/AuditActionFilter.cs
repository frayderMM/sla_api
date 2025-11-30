using Microsoft.AspNetCore.Mvc.Filters;
using DamslaApi.Services;
using System.Security.Claims;

namespace DamslaApi.Utils
{
    public class AuditActionFilter : IAsyncActionFilter
    {
        private readonly LogService _logService;

        public AuditActionFilter(LogService logService)
        {
            _logService = logService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            try
            {
                var http = context.HttpContext;
                var usuarioId = http.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                await _logService.RegistrarLog(
                    usuarioId != null ? int.Parse(usuarioId) : (int?)null,
                    http.Request.Method,
                    http.Request.Path,
                    executed.ActionDescriptor.DisplayName,
                    http.Connection.RemoteIpAddress?.ToString(),
                    http.Request.Headers["User-Agent"]
                );
            }
            catch
            {
                // Nunca romper el flujo si falla el log
            }
        }
    }
}
