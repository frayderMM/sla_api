using Microsoft.EntityFrameworkCore;
using DamslaApi.Models;

namespace DamslaApi.Data
{
    public class DamslaDbContext : DbContext
    {
        public DamslaDbContext(DbContextOptions<DamslaDbContext> options)
            : base(options) {}

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<TipoSla> TiposSla { get; set; }
        public DbSet<LogAcceso> LogAcceso { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
    }
}