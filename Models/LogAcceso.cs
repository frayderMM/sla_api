namespace DamslaApi.Models
{
    public class LogAcceso
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public string Metodo { get; set; }
        public string Endpoint { get; set; }
        public string Accion { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public Usuario Usuario { get; set; }
    }
}