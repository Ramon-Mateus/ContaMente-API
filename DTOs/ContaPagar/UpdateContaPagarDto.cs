using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class UpdateContaPagarDto
    {
        public int? ResponsavelId { get; set; }
        public string? Descricao { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal? ValorTotal { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal? ValorParcela { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? Vencimento { get; set; }
        public int? CategoriaId { get; set; }
        public List<CategoriaRateioDto>? Categorias { get; set; }
    }
}
