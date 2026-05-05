namespace ContaMente.Models
{
    public class ContaPagarCategoria
    {
        public int ContaPagarId { get; set; }
        public ContaPagar ContaPagar { get; set; } = null!;
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }
}
