using ContaMente.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ContaMente.DTOs
{
    public class CreateMovimentacaoDto
    {
        [Required]
        public double Valor { get; set; }
        [Required]
        public DateTime Data { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        [Required]
        public bool Fixa { get; set; }
        [Required]
        public int CategoriaId { get; set; }
        [Required]
        public int TipoPagamentoId { get; set; }
        public int? RecorrenciaId { get; set; }
        public CreateParcelaDto? Parcela { get; set; }
    }
}
