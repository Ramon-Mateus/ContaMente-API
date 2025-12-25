using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class ResponsavelService : IResponsavelService
    {
        private readonly IResponsavelRepository _responsavelRepository;
        private readonly IMovimentacaoService _movimentacaoService;

        public ResponsavelService(IResponsavelRepository responsavelRepository, IMovimentacaoService movimentacaoService)
        {
            _responsavelRepository = responsavelRepository;
            _movimentacaoService = movimentacaoService;
        }

        public async Task<List<Responsavel>> GetResponsaveis(string userId)
        {
            return await _responsavelRepository.GetResponsaveis(userId);
        }

        public async Task<Responsavel?> GetResponsavelById(int id, string userId)
        {
            return await _responsavelRepository.GetResponsavelById(id, userId);
        }

        public async Task<Responsavel> CreateResponsavel(CreateUpdateResponsavelDto createResponsavelDto, string userId)
        {
            var responsavelExiste = await _responsavelRepository.ExisteResponsavelComNome(createResponsavelDto.Nome, userId, null);

            if (responsavelExiste)
            {
                throw new ArgumentException($"Já existe um responsável com o nome '{createResponsavelDto.Nome}'.");
            }

            var responsavel = new Responsavel
            {
                Nome = createResponsavelDto.Nome,
                UserId = userId
            };

            return await _responsavelRepository.CreateResponsavel(responsavel);
        }

        public async Task<Responsavel?> UpdateResponsavel(int id, CreateUpdateResponsavelDto updateResponsavelDto, string userId)
        {
            var responsavel = await this.GetResponsavelById(id, userId);

            if (responsavel == null)
            {
                return null;
            }

            var responsavelExiste = await _responsavelRepository.ExisteResponsavelComNome(updateResponsavelDto.Nome, userId, responsavel.Id);

            if (responsavelExiste)
            {
                throw new ArgumentException($"Já existe um responsável com o nome '{updateResponsavelDto.Nome}'.");
            }

            responsavel.Nome = updateResponsavelDto.Nome;

            return await _responsavelRepository.UpdateResponsavel(responsavel);
        }

        public async Task<bool> DeleteResponsavel(int id, string userId)
        {
            var responsavel = await this.GetResponsavelById(id, userId);

            if (responsavel == null)
            {
                return false;
            }

            for (int i = 0; i < responsavel.Movimentacoes.Count; i++)
            {
                await _movimentacaoService.DeleteMovimentacao(responsavel.Movimentacoes[i].Id, userId);
            }

            return await _responsavelRepository.DeleteResponsavel(responsavel);
        }
    }
}
