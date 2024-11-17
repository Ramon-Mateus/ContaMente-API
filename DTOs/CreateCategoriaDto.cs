using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs
{
    public class CreateCategoriaDto
    {
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
    }
}
