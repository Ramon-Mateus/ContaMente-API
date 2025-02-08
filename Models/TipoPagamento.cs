namespace ContaMente.Models
{
    public class TipoPagamento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
