namespace DamslaApi.Dtos.Dashboard
{
    public class PrediccionSlaDto
    {
        public string Rol { get; set; }
        public double PromedioMeses { get; set; }
        public double Pendiente { get; set; }
        public double Intercepto { get; set; }
        public double PrediccionProximoMes { get; set; }
        public string EstadoEsperado { get; set; }
    }
}
