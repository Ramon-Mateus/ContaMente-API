using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs;

public class CreateGastoDto
{
    [Required]
    public double Valor { get; set; }
    [Required]
    public DateTime Data { get; set; }
    [MaxLength(100)]
    public string? Descricao { get; set; } = string.Empty;
    [Required]
    public int CategoriaId { get; set; }
}