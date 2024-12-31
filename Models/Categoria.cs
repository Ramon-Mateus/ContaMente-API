using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ContaMente.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;
        public List<Gasto> Gastos { get; set; } = new List<Gasto>();
    }
}
