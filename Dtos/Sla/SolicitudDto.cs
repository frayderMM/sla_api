namespace DamslaApi.Dtos.Sla
{
    public class SolicitudDto
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int TipoSlaId { get; set; }
    }
}