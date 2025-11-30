namespace DamslaApi.Dtos.Sla
{
    public class SlaResumenDto
    {
        public string Rol { get; set; }
        public int TotalSolicitudes { get; set; }
        public int Cumplen { get; set; }
        public int NoCumplen { get; set; }
        public double PorcentajeCumplimiento { get; set; }
        public string IndicadorColor { get; set; }   // green / red / gray
    }
}