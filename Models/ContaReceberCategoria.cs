namespace ContaMente.Models
{
    public class ContaReceberCategoria
    {
        public int ContaReceberId { get; set; }
        public ContaReceber ContaReceber { get; set; } = null!;
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }
}
