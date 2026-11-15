using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.DTOs.Pluggy;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services.Pluggy
{
    public interface ITransacaoPendenteService
    {
        Task<List<TransacaoPendenteDto>> Listar(string userId, StatusTransacaoPendente status);
        Task<int> ContarPendentes(string userId);
        Task<Movimentacao> Aprovar(int id, CreateMovimentacaoDto dto, string userId);
        Task<Parcela> AprovarParcelada(int id, CreateParcelaDto dto, string userId);
        Task AlterarStatus(int id, StatusTransacaoPendente novoStatus, string userId);
    }

    public class TransacaoPendenteService : ITransacaoPendenteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMovimentacaoService _movimentacaoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IParcelaService _parcelaService;

        public TransacaoPendenteService(ApplicationDbContext context, IMovimentacaoService movimentacaoService, ICategoriaService categoriaService, IParcelaService parcelaService)
        {
            _context = context;
            _movimentacaoService = movimentacaoService;
            _categoriaService = categoriaService;
            _parcelaService = parcelaService;
        }

        public async Task<List<TransacaoPendenteDto>> Listar(string userId, StatusTransacaoPendente status)
        {
            var vinculos = await _context.PluggyContas
                .Where(c => c.Item!.UserId == userId && c.CartaoId != null)
                .ToDictionaryAsync(c => c.PluggyAccountId, c => c.CartaoId);

            var query = _context.TransacoesPendentes.Where(t => t.UserId == userId && t.Status == status);
            query = status == StatusTransacaoPendente.Pendente
                ? query.OrderByDescending(t => t.Data)
                : query.OrderByDescending(t => t.Data).Take(200);

            var transacoes = await query.ToListAsync();

            return transacoes.Select(t => new TransacaoPendenteDto
            {
                Id = t.Id,
                Descricao = t.Descricao,
                Valor = t.Valor,
                Data = t.Data,
                NomeConta = t.NomeConta,
                FinalCartao = t.FinalCartao,
                CategoriaPluggy = t.CategoriaPluggy,
                NumeroParcela = t.NumeroParcela,
                TotalParcelas = t.TotalParcelas,
                ValorTotal = t.ValorTotal,
                ParcelaId = t.ParcelaId,
                Pendente = t.Pendente,
                Status = (int)t.Status,
                MovimentacaoId = t.MovimentacaoId,
                CartaoSugeridoId = vinculos.TryGetValue(t.PluggyAccountId, out var cartaoId) ? cartaoId : null,
            }).ToList();
        }

        public Task<int> ContarPendentes(string userId) =>
            _context.TransacoesPendentes.CountAsync(t => t.UserId == userId && t.Status == StatusTransacaoPendente.Pendente);

        private async Task<TransacaoPendente> ValidarAprovacao(int id, string userId, int categoriaId, int? cartaoId, int? responsavelId)
        {
            var transacao = await _context.TransacoesPendentes.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
                ?? throw new KeyNotFoundException("Transação importada não encontrada.");

            if (transacao.Status != StatusTransacaoPendente.Pendente)
                throw new ArgumentException("Essa transação já foi aprovada ou ignorada.");

            _ = await _categoriaService.GetCategoriaById(categoriaId, userId)
                ?? throw new KeyNotFoundException("Categoria não encontrada.");

            if (cartaoId.HasValue && !await _context.Cartoes.AnyAsync(c => c.Id == cartaoId && c.UserId == userId))
                throw new KeyNotFoundException("Cartão não encontrado.");

            if (responsavelId.HasValue && !await _context.Responsaveis.AnyAsync(r => r.Id == responsavelId && r.UserId == userId))
                throw new KeyNotFoundException("Responsável não encontrado.");

            return transacao;
        }

        // Compra parcelada: cria a Parcela (e todas as movimentações) pelo mesmo fluxo do cadastro manual.
        public async Task<Parcela> AprovarParcelada(int id, CreateParcelaDto dto, string userId)
        {
            var transacao = await ValidarAprovacao(id, userId, dto.CategoriaId, dto.CartaoId, dto.ResponsavelId);

            if (dto.NumeroParcelas < 2)
                throw new ArgumentException("Uma compra parcelada precisa de pelo menos 2 parcelas.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            var parcela = await _parcelaService.CreateParcela(dto, userId);

            transacao.Status = StatusTransacaoPendente.Aprovada;
            transacao.ParcelaId = parcela.Id;
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return parcela;
        }

        public async Task<Movimentacao> Aprovar(int id, CreateMovimentacaoDto dto, string userId)
        {
            var transacao = await ValidarAprovacao(id, userId, dto.CategoriaId, dto.CartaoId, dto.ResponsavelId);

            // Movimentação importada é sempre avulsa: cada parcela chega separada na fatura.
            dto.Fixa = false;
            dto.ParcelaId = null;

            await using var tx = await _context.Database.BeginTransactionAsync();

            var movimentacao = await _movimentacaoService.CreateMovimentacao(dto);

            transacao.Status = StatusTransacaoPendente.Aprovada;
            transacao.MovimentacaoId = movimentacao.Id;
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return movimentacao;
        }

        public async Task AlterarStatus(int id, StatusTransacaoPendente novoStatus, string userId)
        {
            var transacao = await _context.TransacoesPendentes.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId)
                ?? throw new KeyNotFoundException("Transação importada não encontrada.");

            if (transacao.Status == StatusTransacaoPendente.Aprovada)
                throw new ArgumentException("Transação já aprovada não pode mudar de status.");

            transacao.Status = novoStatus;
            await _context.SaveChangesAsync();
        }
    }
}
