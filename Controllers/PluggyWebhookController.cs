using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ContaMente.Services.Pluggy;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaMente.Controllers
{
    [Route("api/webhooks/pluggy")]
    [ApiController]
    [AllowAnonymous]
    public class PluggyWebhookController : ControllerBase
    {
        private static readonly HashSet<string> _eventosDeSync = new()
        {
            "item/created", "item/updated", "transactions/created", "transactions/updated", "transactions/deleted"
        };

        private readonly IBackgroundJobClient _jobs;
        private readonly ILogger<PluggyWebhookController> _logger;

        public PluggyWebhookController(IBackgroundJobClient jobs, ILogger<PluggyWebhookController> logger)
        {
            _jobs = jobs;
            _logger = logger;
        }

        // O Pluggy exige resposta 2XX em até 5s: só enfileiramos o trabalho no Hangfire.
        [HttpPost]
        public IActionResult Receber([FromBody] JsonElement evento, [FromQuery] string? secret)
        {
            var esperado = Environment.GetEnvironmentVariable("PLUGGY_WEBHOOK_SECRET");
            if (!string.IsNullOrEmpty(esperado) &&
                (secret == null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(esperado))))
                return Unauthorized();

            var nome = evento.TryGetProperty("event", out var e) ? e.GetString() : null;
            var itemId = evento.TryGetProperty("itemId", out var i) ? i.GetString() : null;

            _logger.LogInformation("Pluggy webhook: {Evento} item {ItemId}", nome, itemId);

            if (nome != null && itemId != null && (_eventosDeSync.Contains(nome) || nome == "item/error"))
                _jobs.Enqueue<IPluggySyncService>(s => s.SincronizarItem(itemId));

            return Ok(new { received = true });
        }
    }
}
