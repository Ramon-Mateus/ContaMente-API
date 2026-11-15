using System.Text.Json.Serialization;

// Conta de cartão de crédito retornada pelo Pluggy, opcionalmente vinculada a um Cartao do sistema.
public class PluggyConta
{
    public int Id { get; set; }
    public string PluggyAccountId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Numero { get; set; }
    [JsonIgnore]
    public int ItemId { get; set; }
    [JsonIgnore]
    public PluggyItem? Item { get; set; }
    public int? CartaoId { get; set; }
    [JsonIgnore]
    public Cartao? Cartao { get; set; }
}
