namespace ContaMente.DTOs
{
    public class ContaReceberDto
    {
        public int Id { get; set; }
        public Guid GrupoLancamentoId { get; set; }
        public ResponsavelDto Responsavel { get; set; } = null!;
        public string? Descricao { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorParcela { get; set; }
        public decimal ValorBaixado { get; set; }
        public decimal ValorRestante { get; set; }
        public int NumeroParcelas { get; set; }
        public int NumeroDaParcela { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime Vencimento { get; set; }
        public StatusDuplicataEnum Status { get; set; }
        public bool Pago { get; set; }
        public DateTime? DataPagamento { get; set; }
        public CategoriaDto Categoria { get; set; } = null!;
        public List<CategoriaRateioDetalheDto> Categorias { get; set; } = new();
    }
}
