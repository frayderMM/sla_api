namespace DamslaApi.Models
{
    public class TipoSla
    {
        public int Id { get; set; }
        public string Codigo { get; set; }  // SLA1, SLA2, SLA3, etc.
        public string Descripcion { get; set; }
        public int TiempoRespuesta { get; set; }  // Días - Ahora configurable desde DB
        
        // Propiedades computadas para Android (compatibilidad)
        public string Nombre => Codigo;
    }
}