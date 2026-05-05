namespace ContaMente.Models
{
    public class ContaReceber
    {
        public int Id { get; set; }
        public Guid GrupoLancamentoId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ResponsavelId { get; set; }
        public Responsavel Responsavel { get; set; } = null!;
        public string? Descricao { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorParcela { get; set; }
        public decimal ValorBaixado { get; set; }
        public decimal ValorRestante { get; set; }
        public int NumeroParcelas { get; set; }
        public int NumeroDaParcela { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime Vencimento { get; set; }
        public StatusDuplicataEnum Status { get; set; } = StatusDuplicataEnum.Aberta;
        public DateTime? DataPagamento { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
        public User User { get; set; } = null!;
        public List<ContaReceberMovimentacao> Movimentacoes { get; set; } = new();
        public List<ContaReceberCategoria> CategoriasRateio { get; set; } = new();
    }
}
