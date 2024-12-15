using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    public class GastoService : IGastoService
    {
        private readonly IGastoRepository _gastoRepository;
        public GastoService(IGastoRepository gastoRepository) => _gastoRepository = gastoRepository;

        public async Task<List<Gasto>> GetGastos()
        {
            return await _gastoRepository.GetGastos();
        }

        public async Task<Gasto?> GetGastoById(int id)
        {
            return await _gastoRepository.GetGastoById(id);
        }

        public async Task<Gasto> CreateGasto(CreateGastoDto createGastoDto)
        {
            var gasto = new Gasto
            {
                Valor = createGastoDto.Valor,
                Descricao = createGastoDto.Descricao,
                Data = createGastoDto.Data,
                CategoriaId = createGastoDto.CategoriaId
            };

            return await _gastoRepository.CreateGasto(gasto);
        }

        public async Task<Gasto?> UpdateGasto(int id, UpdateGastoDto updateGastoDto)
        {
            var gasto = await this.GetGastoById(id);

            if (gasto == null)
            {
                return null;
            }

            if (updateGastoDto.Valor.HasValue)
            {
                gasto.Valor = updateGastoDto.Valor.Value;
            }

            if (updateGastoDto.Data.HasValue)
            {
                gasto.Data = updateGastoDto.Data.Value;
            }

            if (!string.IsNullOrEmpty(updateGastoDto.Descricao))
            {
                gasto.Descricao = updateGastoDto.Descricao;
            }

            if (updateGastoDto.CategoriaId.HasValue)
            {
                gasto.CategoriaId = updateGastoDto.CategoriaId.Value;
            }

            return await _gastoRepository.UpdateGasto(gasto);
        }

        public async Task<bool> DeleteGasto(int id)
        {
            var gasto = await this.GetGastoById(id);

            if (gasto == null)
            {
                return false;
            }

            return await _gastoRepository.DeleteGasto(gasto);
        }
    }
}