using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class UpdateCategoriaDto
    {
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
    }
}
