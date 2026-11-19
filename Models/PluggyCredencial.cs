using ContaMente.Models;

// Credenciais da conta Pluggy do usuário (cada usuário usa a sua própria conta em dashboard.pluggy.ai).
// O client secret fica criptografado (AES-GCM) com a chave CREDENCIAIS_CHAVE.
public class PluggyCredencial
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecretCriptografado { get; set; } = string.Empty;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
}
