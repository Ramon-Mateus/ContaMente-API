namespace ContaMente.DTOs
{
    public class CategoriaRateioDetalheDto
    {
        public CategoriaDto Categoria { get; set; } = null!;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }
}
