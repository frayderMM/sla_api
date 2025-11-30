namespace DamslaApi.Dtos.DashboardPrincipal;

public class HistoricoDto
{
    public string TipoSla { get; set; }
    public List<PuntoHistoricoDto> Historico { get; set; }
}

public class PuntoHistoricoDto
{
    public string Periodo { get; set; }
    public double Porcentaje { get; set; }
}
