namespace ContaMente.Models
{
    public class MovimentacaoCategoria
    {
        public int MovimentacaoId { get; set; }
        public Movimentacao Movimentacao { get; set; } = null!;
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }
}
