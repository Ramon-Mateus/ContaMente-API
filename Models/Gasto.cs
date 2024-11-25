using System.Text.Json.Serialization;

namespace ContaMente.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public double Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        [JsonIgnore]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
