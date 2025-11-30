namespace DamslaApi.Dtos.DashboardPrincipal;

public class IndicadoresDto
{
    public string TipoSla { get; set; }
    public int Total { get; set; }
    public int Cumple { get; set; }
    public int NoCumple { get; set; }
    public double PorcentajeCumplimiento { get; set; }
    public double PromedioDias { get; set; }
}
