namespace ContaMente.DTOs
{
    public class UpdateMovimentacaoDto
    {
        public double? Valor { get; set; }
        public DateTime? Data { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        public bool? Fixa { get; set; }
        public int? CategoriaId { get; set; }
        public int? TipoPagamentoId { get; set; }
        public int? ResponsavelId { get; set; }
    }
}
