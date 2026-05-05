using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class BaixaContaPagarDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public int TipoPagamentoId { get; set; }

        public int? ResponsavelId { get; set; }
        public int? CartaoId { get; set; }
    }
}
