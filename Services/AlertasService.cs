using DamslaApi.Data;
using DamslaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class AlertasService
    {
        private readonly DamslaDbContext _db;
        private readonly DashboardService _dashboard;
        private readonly EmailService _email;

        public AlertasService(DamslaDbContext db, DashboardService dashboard, EmailService email)
        {
            _db = db;
            _dashboard = dashboard;
            _email = email;
        }

        // Genera alertas para el mes actual
        public async Task GenerarAlertas()
        {
            int year = DateTime.UtcNow.Year;

            var dashboard = await _dashboard.GetDashboardMensual(year);
            var mesActual = DateTime.UtcNow.Month.ToString("00") + "-" + year;

            var mesData = dashboard.Where(x => x.Mes == mesActual);

            foreach (var dato in mesData)
            {
                if (dato.Porcentaje < 60)
                {
                    // Verificar si ya existe una alerta para este rol en el mes actual
                    var existeAlerta = await _db.Alertas
                        .AnyAsync(a => a.Rol == dato.Rol && 
                                      a.Fecha.Month == DateTime.UtcNow.Month &&
                                      a.Fecha.Year == DateTime.UtcNow.Year &&
                                      !a.Atendida);

                    if (!existeAlerta)
                    {
                        string mensaje = $"⚠️ ALERTA SLA: El rol {dato.Rol} tiene un cumplimiento de {dato.Porcentaje}% este mes.";

                        // Guardar en base de datos
                        var alerta = new Alerta
                        {
                            Rol = dato.Rol,
                            Porcentaje = dato.Porcentaje,
                            Mensaje = mensaje,
                            Fecha = DateTime.UtcNow,
                            Atendida = false
                        };

                        _db.Alertas.Add(alerta);
                        await _db.SaveChangesAsync();

                        // Enviar correo
                        _email.EnviarCorreo(
                            "tcs-alertas@esan.edu.pe",
                            "⚠️ Alerta SLA Crítico",
                            mensaje
                        );
                    }
                }
            }
        }

        // Obtener alertas activas
        public async Task<List<Alerta>> ObtenerAlertas()
        {
            return await _db.Alertas
                .OrderByDescending(a => a.Fecha)
                .Take(100)
                .ToListAsync();
        }

        // Marcar alerta como atendida
        public async Task<bool> MarcarComoAtendida(int id)
        {
            var alerta = await _db.Alertas.FindAsync(id);
            if (alerta == null) return false;

            alerta.Atendida = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
