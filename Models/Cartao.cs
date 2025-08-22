using System.Text.Json.Serialization;
using ContaMente.Models;

public class Cartao
{
    public int Id { get; set; }
    public string Apelido { get; set; } = string.Empty;
    public int DiaFechamento { get; set; }
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public List<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
}