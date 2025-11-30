namespace DamslaApi.Models
{
    public class TipoSla
    {
        public int Id { get; set; }
        public string Codigo { get; set; }  // SLA1, SLA2
        public string Descripcion { get; set; }
        
        // Propiedades computadas para Android
        public string Nombre => Codigo;
        public int TiempoRespuesta => Codigo == "SLA1" ? 35 : 20;  // días
    }
}