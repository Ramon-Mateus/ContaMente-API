namespace ContaMente.Models
{
    public class Recorrencia
    {
        public int Id { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; } = null;
        public List<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
