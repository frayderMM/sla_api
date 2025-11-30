using DamslaApi.Dtos.Dashboard;

namespace DamslaApi.Services
{
    public class PrediccionService
    {
        public PrediccionSlaDto Predecir(IEnumerable<DashboardMensualDto> historico)
        {
            var lista = historico.OrderBy(x => x.Mes).ToList();
            int n = lista.Count;

            if (n < 2)
                return new PrediccionSlaDto
                {
                    Rol = lista.First().Rol,
                    PrediccionProximoMes = lista.Average(x => x.Porcentaje),
                    EstadoEsperado = "Insuficiente data"
                };

            // x = mes en número (1...12)
            // y = % cumplimiento

            var x = Enumerable.Range(1, n).Select(i => (double)i).ToList();
            var y = lista.Select(i => i.Porcentaje).ToList();

            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            double m = (n * sumXY - (sumX * sumY)) / (n * sumX2 - (sumX * sumX));
            double b = (sumY - m * sumX) / n;

            double nextX = n + 1;  
            double pred = Math.Round(m * nextX + b, 2);

            return new PrediccionSlaDto
            {
                Rol = lista.First().Rol,
                PromedioMeses = lista.Average(i => i.Porcentaje),
                Pendiente = m,
                Intercepto = b,
                PrediccionProximoMes = pred,
                EstadoEsperado = pred >= 80 ? "Alta probabilidad de cumplimiento"
                                : pred >= 60 ? "Posible riesgo"
                                : "Riesgo alto"
            };
        }
    }
}
