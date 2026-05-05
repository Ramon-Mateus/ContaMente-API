namespace ContaMente.Models
{
    public class ContaPagarMovimentacao
    {
        public int ContaPagarId { get; set; }
        public ContaPagar ContaPagar { get; set; } = null!;
        public int MovimentacaoId { get; set; }
        public Movimentacao Movimentacao { get; set; } = null!;
        public decimal ValorBaixado { get; set; }
        public DateTime DataBaixa { get; set; }
    }
}
