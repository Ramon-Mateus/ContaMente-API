using System.Security.Claims;
using ContaMente.Contexts;
using ContaMente.DTOs.Pluggy;
using ContaMente.Services.Pluggy;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PluggyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPluggyClient _pluggy;
        private readonly IPluggySyncService _sync;
        private readonly IBackgroundJobClient _jobs;

        public PluggyController(ApplicationDbContext context, IPluggyClient pluggy, IPluggySyncService sync, IBackgroundJobClient jobs)
        {
            _context = context;
            _pluggy = pluggy;
            _sync = sync;
            _jobs = jobs;
        }

        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        [HttpPost("connect-token")]
        public async Task<IActionResult> CreateConnectToken([FromBody] ConnectTokenRequestDto? request)
        {
            string? itemId = null;
            if (!string.IsNullOrWhiteSpace(request?.ItemId))
            {
                // Modo atualização: só para conexões do próprio usuário.
                var existe = await _context.PluggyItens.AnyAsync(i => i.PluggyItemId == request.ItemId && i.UserId == UserId);
                if (!existe) throw new KeyNotFoundException("Conexão não encontrada.");
                itemId = request.ItemId;
            }

            var token = await _pluggy.CreateConnectToken(UserId, itemId);
            return Ok(new { accessToken = token });
        }

        [HttpGet("itens")]
        public async Task<IActionResult> GetItens()
        {
            var itens = await _context.PluggyItens
                .Include(i => i.Contas)
                .Where(i => i.UserId == UserId)
                .OrderBy(i => i.CriadoEm)
                .ToListAsync();

            return Ok(itens);
        }

        // Chamado pelo front no onSuccess do Pluggy Connect.
        [HttpPost("itens")]
        public async Task<IActionResult> RegistrarItem([FromBody] RegistrarItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                throw new ArgumentException("ItemId é obrigatório.");

            var apiItem = await _pluggy.GetItem(dto.ItemId);

            // O connect token é criado com clientUserId = id do usuário; impede registrar item de outra pessoa.
            if (!string.IsNullOrEmpty(apiItem.ClientUserId) && apiItem.ClientUserId != UserId)
                throw new UnauthorizedAccessException("Essa conexão não pertence ao usuário logado.");

            var item = await _context.PluggyItens.FirstOrDefaultAsync(i => i.PluggyItemId == dto.ItemId);
            if (item != null && item.UserId != UserId)
                throw new UnauthorizedAccessException("Essa conexão não pertence ao usuário logado.");

            if (item == null)
            {
                item = new PluggyItem { PluggyItemId = dto.ItemId, UserId = UserId };
                _context.PluggyItens.Add(item);
            }

            item.NomeConector = apiItem.Connector?.Name;
            item.Status = apiItem.Status;
            item.ErroUltimaAtualizacao = apiItem.Error?.Message;
            await _context.SaveChangesAsync();

            _jobs.Enqueue<IPluggySyncService>(s => s.SincronizarItem(dto.ItemId));

            return Ok(item);
        }

        [HttpDelete("itens/{id:int}")]
        public async Task<IActionResult> RemoverItem(int id)
        {
            var item = await _context.PluggyItens.FirstOrDefaultAsync(i => i.Id == id && i.UserId == UserId)
                ?? throw new KeyNotFoundException("Conexão não encontrada.");

            await _pluggy.DeleteItem(item.PluggyItemId);

            var contas = await _context.PluggyContas.Where(c => c.ItemId == item.Id).Select(c => c.PluggyAccountId).ToListAsync();
            var pendentes = _context.TransacoesPendentes
                .Where(t => t.UserId == UserId && contas.Contains(t.PluggyAccountId) && t.Status == StatusTransacaoPendente.Pendente);
            _context.TransacoesPendentes.RemoveRange(pendentes);
            _context.PluggyItens.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("contas/{id:int}/cartao")]
        public async Task<IActionResult> VincularCartao(int id, [FromBody] VincularCartaoDto dto)
        {
            var conta = await _context.PluggyContas.FirstOrDefaultAsync(c => c.Id == id && c.Item!.UserId == UserId)
                ?? throw new KeyNotFoundException("Conta não encontrada.");

            if (dto.CartaoId.HasValue && dto.CartaoId != 0 &&
                !await _context.Cartoes.AnyAsync(c => c.Id == dto.CartaoId && c.UserId == UserId))
                throw new KeyNotFoundException("Cartão não encontrado.");

            conta.CartaoId = dto.CartaoId == 0 ? null : dto.CartaoId;
            await _context.SaveChangesAsync();

            return Ok(conta);
        }

        // Pede ao Pluggy para atualizar os dados no banco e já importa o que estiver disponível.
        [HttpPost("sincronizar")]
        public async Task<IActionResult> Sincronizar()
        {
            var itens = await _context.PluggyItens.Where(i => i.UserId == UserId).Select(i => i.PluggyItemId).ToListAsync();

            foreach (var itemId in itens)
            {
                try { await _pluggy.UpdateItem(itemId); } catch (HttpRequestException) { /* ex.: atualização já em andamento */ }
                await _sync.SincronizarItem(itemId);
            }

            return Ok(new { itens = itens.Count });
        }
    }
}
