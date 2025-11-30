namespace DamslaApi.Dtos.Sla
{
    public class MetricsGlobDto
    {
        public int TotalSolicitudes { get; set; }
        public int Cumplen { get; set; }
        public int NoCumplen { get; set; }
        public double PorcentajeCumplidos { get; set; }
        public double PorcentajeNoCumplidos { get; set; }
        public int RetrasoMaximoDias { get; set; }
        public double RetrasoPromedioDias { get; set; }
    }
}
