using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ContaMente.Models;

public class UserConfiguration
{
    [Key]
    public int Id { get; set; }

    [Required]
    public bool ListagemPorFatura { get; set; } // Caso seja falsa, será listado por mês

    [Required]
    [JsonIgnore]
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;
    
    public User User { get; set; } = new User();
}
