namespace DamslaApi.Dtos.TiposSla
{
    public class UpdateTipoSlaDto
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public int TiempoRespuesta { get; set; }  // Días
    }
}
