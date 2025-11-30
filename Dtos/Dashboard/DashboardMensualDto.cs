namespace DamslaApi.Dtos.Dashboard
{
    public class DashboardMensualDto
    {
        public string Mes { get; set; }
        public string Rol { get; set; }
        public int Total { get; set; }
        public int Cumplen { get; set; }
        public int NoCumplen { get; set; }
        public double Porcentaje { get; set; }
        public string Color { get; set; }
    }
}
