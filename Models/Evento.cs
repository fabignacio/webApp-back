namespace apiDocument.Models
{
    public class Evento
    {
        public int? ID { get; set; }
        public required DateTime Fecha { get; set; }
        public required string Descripcion { get; set; }
        public required string Impacto { get; set; }

        public string? RutaImagen { get; set;}

        public string? ImagenBase64 { get; set; }  
        public byte[]? RegistroFotografico { get; set; }
        public required string AccionesInmediatas { get; set; }
        public required string AtencionEvento { get; set; }

        //Datos Antecedentes
        public required string Operacion { get; set; }
        public required DateTime FechaAntecedente { get; set; }
        public required string Ubicacion { get; set; }
        public TimeSpan? HoraInformada { get; set; }

        //Datos PersonalInvolucrado
        public required string Nombre { get; set; }
        public required string Cargo { get; set; }
        public required string DanioGenerado { get; set; }
        public required string Equipos { get; set; }
    }
}
