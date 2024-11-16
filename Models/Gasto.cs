using System.Text.Json.Serialization;

namespace ContaMente.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public double Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        [JsonIgnore]
        public Categoria? Categoria { get; set; }
    }
}
