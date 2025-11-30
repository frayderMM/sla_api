using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DamslaApi.Services
{
    public class BackgroundAlertTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundAlertTask> _logger;

        public BackgroundAlertTask(IServiceProvider serviceProvider, ILogger<BackgroundAlertTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundAlertTask iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var alertaService = scope.ServiceProvider.GetRequiredService<AlertasService>();
                        await alertaService.GenerarAlertas();
                        _logger.LogInformation("Alertas procesadas automáticamente");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar alertas automáticas");
                }

                // Esperar 24 horas
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
