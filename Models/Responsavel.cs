namespace ContaMente.Models
{
    public class Responsavel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public List<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
