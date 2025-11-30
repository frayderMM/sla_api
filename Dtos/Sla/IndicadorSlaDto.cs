namespace DamslaApi.Dtos.Sla
{
    public class IndicadorSlaDto
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public string TipoSla { get; set; }
        public int Dias { get; set; }
        public string Resultado { get; set; }     // Cumple / No Cumple
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaIngreso { get; set; }
    }
}