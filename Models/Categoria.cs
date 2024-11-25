using System.Text.Json.Serialization;

namespace ContaMente.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        [JsonIgnore]
        public List<Gasto> Gastos { get; set; } = new List<Gasto>();
    }
}
