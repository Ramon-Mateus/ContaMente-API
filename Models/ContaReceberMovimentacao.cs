namespace ContaMente.Models
{
    public class ContaReceberMovimentacao
    {
        public int ContaReceberId { get; set; }
        public ContaReceber ContaReceber { get; set; } = null!;
        public int MovimentacaoId { get; set; }
        public Movimentacao Movimentacao { get; set; } = null!;
        public decimal ValorBaixado { get; set; }
        public DateTime DataBaixa { get; set; }
    }
}
