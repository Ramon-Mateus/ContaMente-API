using System.Text.Json.Serialization;
using ContaMente.Models;

// Uma conexão (instituição) feita pelo Pluggy Connect.
public class PluggyItem
{
    public int Id { get; set; }
    public string PluggyItemId { get; set; } = string.Empty;
    public string? NomeConector { get; set; }
    public string? Status { get; set; }
    public string? ErroUltimaAtualizacao { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? UltimaSincronizacao { get; set; }
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    [JsonIgnore]
    public User? User { get; set; }
    public List<PluggyConta> Contas { get; set; } = new();
}
