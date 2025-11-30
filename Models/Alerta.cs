namespace DamslaApi.Models
{
    public class Alerta
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public double Porcentaje { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public bool Atendida { get; set; }
    }
}
