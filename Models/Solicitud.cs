using System.Text.Json.Serialization;

namespace DamslaApi.Models
{
    public class Solicitud
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public int TipoSlaId { get; set; }
        public TipoSla TipoSla { get; set; }
        public int UsuarioId { get; set; }
        
        [JsonIgnore]
        public Usuario Usuario { get; set; }
        
        public int? CreadoPor { get; set; }
        
        // Propiedades computadas para Android
        public string Descripcion => Rol;
        public string Estado => FechaIngreso.HasValue ? "Cerrado" : "Abierto";
        public DateTime FechaCreacion => FechaSolicitud;
        
        // DTO para Usuario simplificado
        public UsuarioSimpleDto? UsuarioDto => Usuario != null ? new UsuarioSimpleDto
        {
            Id = Usuario.Id,
            NombreCompleto = Usuario.Nombre,
            Email = Usuario.Email
        } : null;
    }
    
    public class UsuarioSimpleDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
    }
}