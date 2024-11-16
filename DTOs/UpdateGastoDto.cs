using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class UpdateGastoDto
    {
        public double? Valor { get; set; }
        public DateTime? Data { get; set; }
        [MaxLength(100)]
        public string? Descricao { get; set; } = string.Empty;
        public int? CategoriaId { get; set; }
    }
}
