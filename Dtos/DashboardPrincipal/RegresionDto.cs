namespace DamslaApi.Dtos.DashboardPrincipal;

public class RegresionDto
{
    public string TipoSla { get; set; }
    public RegresionLinealDto Regresion { get; set; }
    public List<PuntoRegresionDto> Historico { get; set; }
    public ProyeccionDto Proyeccion { get; set; }
    public double PromedioDias { get; set; }
    public string Recomendacion { get; set; }
}

public class RegresionLinealDto
{
    public double Pendiente { get; set; }
    public double Intercepto { get; set; }
    public double R2 { get; set; }
}

public class PuntoRegresionDto
{
    public int X { get; set; }
    public double Y { get; set; }
}

public class ProyeccionDto
{
    public string PeriodoSiguiente { get; set; }
    public double Valor { get; set; }
    public string NivelRiesgo { get; set; }
}
