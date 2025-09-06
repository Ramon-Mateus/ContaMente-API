namespace ContaMente.DTOs
{
    public class RecorrenciaDto
    {
        public int Id { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; } = null;
    }
}