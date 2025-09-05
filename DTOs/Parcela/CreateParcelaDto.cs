namespace ContaMente.DTOs
{
    public class CreateParcelaDto
    {
        public double ValorTotal { get; set; }
        public int NumeroParcelas { get; set; }
        public double ValorParcela { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public int CategoriaId { get; set; }
        public int TipoPagamentoId { get; set; }
        public int? ResponsavelId { get; set; }
        public int? CartaoId { get; set; }
    }
}
