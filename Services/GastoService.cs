using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using ContaMente.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services
{
    public class GastoService : IGastoService
    {
        private readonly ApplicationDbContext _context;
        public GastoService(ApplicationDbContext context) => _context = context;

        public async Task<List<Gasto>> GetGastos()
        {
            var gastos = await _context.Gastos
                .Include(g => g.Categoria)
                .OrderByDescending(g => g.Data)
                .ToListAsync();

            return gastos;
        }

        public async Task<Gasto?> GetGastoById(int id)
        {
            var gasto = await _context.Gastos.Include(g => g.Categoria).FirstOrDefaultAsync(g => g.Id == id);

            return gasto;
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

            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

            return gasto;
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

            await _context.SaveChangesAsync();

            return gasto;
        }

        public async Task<bool> DeleteGasto(int id)
        {
            var gasto = await this.GetGastoById(id);

            if (gasto == null)
            {
                return false;
            }

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}