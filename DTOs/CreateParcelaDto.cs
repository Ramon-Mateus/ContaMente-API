namespace ContaMente.DTOs
{
    public class CreateParcelaDto
    {
        public double ValorTotal { get; set; }
        public int NumeroParcelas { get; set; }
        public double ValorParcela { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; } = null;
    }
}
