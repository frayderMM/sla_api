namespace DamslaApi.Dtos.Solicitudes
{
    public class UpdateSolicitudDto
    {
        public string Rol { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int TipoSlaId { get; set; }
    }
}