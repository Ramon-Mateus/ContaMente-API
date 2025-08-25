using System.ComponentModel.DataAnnotations;

public class CreateUpdateCartaoDto
{
    [Required]
    [MaxLength(20)]
    public string Apelido { get; set; } = string.Empty;

    [Required]
    [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31.")]
    public int DiaFechamento { get; set; }
}
