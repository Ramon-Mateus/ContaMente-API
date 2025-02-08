namespace ContaMente.Models
{
    public class Parcela
    {
        public int Id { get; set; }
        public double ValorTotal { get; set; }
        public int NumeroParcelas { get; set; }
        public double ValorParcela { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; } = null;
        public List<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
