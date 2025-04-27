using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ContaMente.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Entrada { get; set; }
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public List<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    }
}
