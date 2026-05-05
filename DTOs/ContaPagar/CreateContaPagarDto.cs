using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class CreateContaPagarDto
    {
        [Required]
        public int ResponsavelId { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public decimal ValorTotal { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroParcelas { get; set; }

        [Required]
        public DateTime DataEmissao { get; set; }

        [Required]
        public DateTime PrimeiroVencimento { get; set; }

        [Required]
        public TipoIntervaloEnum TipoIntervalo { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Intervalo { get; set; }

        public int? CategoriaId { get; set; }
        public List<CategoriaRateioDto>? Categorias { get; set; }
    }
}
