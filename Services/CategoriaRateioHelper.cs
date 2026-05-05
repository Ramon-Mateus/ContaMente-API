using ContaMente.Contexts;
using ContaMente.DTOs;
using ContaMente.Models;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services
{
    internal class CategoriaRateioCalculado
    {
        public Categoria Categoria { get; set; } = null!;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }

    internal static class CategoriaRateioHelper
    {
        public static async Task<List<CategoriaRateioCalculado>> CalcularRateio(
            ApplicationDbContext context,
            List<CategoriaRateioDto>? categorias,
            int? categoriaId,
            decimal valorTotal,
            string userId,
            bool entradaEsperada)
        {
            if (valorTotal <= 0)
                throw new ArgumentException("O valor total do rateio deve ser maior que zero.");

            var entradas = NormalizarEntradas(categorias, categoriaId);
            var categoriasIds = entradas.Select(c => c.CategoriaId).ToList();

            if (categoriasIds.Count != categoriasIds.Distinct().Count())
                throw new ArgumentException("Não é permitido repetir a mesma categoria no rateio.");

            var categoriasDb = await context.Categorias
                .Where(c => categoriasIds.Contains(c.Id) && c.UserId == userId)
                .ToListAsync();

            if (categoriasDb.Count != categoriasIds.Count)
                throw new KeyNotFoundException("Uma ou mais categorias informadas não foram encontradas.");

            if (categoriasDb.Any(c => c.Entrada != entradaEsperada))
                throw new ArgumentException(entradaEsperada
                    ? "Todas as categorias devem ser de entrada."
                    : "Todas as categorias devem ser de saída.");

            return CalcularValores(entradas, categoriasDb, valorTotal);
        }

        public static List<CategoriaRateioDetalheDto> MapearDetalhes<T>(
            IEnumerable<T> rateios,
            Func<T, Categoria> categoriaSelector,
            Func<T, decimal> valorSelector,
            Func<T, decimal> percentualSelector)
        {
            return rateios
                .Select(r => new CategoriaRateioDetalheDto
                {
                    Categoria = new CategoriaDto
                    {
                        Id = categoriaSelector(r).Id,
                        Nome = categoriaSelector(r).Nome,
                        Entrada = categoriaSelector(r).Entrada
                    },
                    Valor = valorSelector(r),
                    Percentual = percentualSelector(r)
                })
                .ToList();
        }

        private static List<CategoriaRateioDto> NormalizarEntradas(
            List<CategoriaRateioDto>? categorias,
            int? categoriaId)
        {
            if (categorias != null && categorias.Count > 0)
                return categorias;

            if (categoriaId.HasValue)
            {
                return new List<CategoriaRateioDto>
                {
                    new() { CategoriaId = categoriaId.Value }
                };
            }

            throw new ArgumentException("Informe pelo menos uma categoria.");
        }

        private static List<CategoriaRateioCalculado> CalcularValores(
            List<CategoriaRateioDto> entradas,
            List<Categoria> categoriasDb,
            decimal valorTotal)
        {
            var temValor = entradas.Any(e => e.Valor.HasValue);
            var temPercentual = entradas.Any(e => e.Percentual.HasValue);

            if (temValor && temPercentual)
                throw new ArgumentException("Informe o rateio por valor ou por percentual, não ambos.");

            if (!temValor && !temPercentual)
                return CalcularRateioIgual(entradas, categoriasDb, valorTotal);

            if (temValor)
                return CalcularRateioPorValor(entradas, categoriasDb, valorTotal);

            return CalcularRateioPorPercentual(entradas, categoriasDb, valorTotal);
        }

        private static List<CategoriaRateioCalculado> CalcularRateioIgual(
            List<CategoriaRateioDto> entradas,
            List<Categoria> categoriasDb,
            decimal valorTotal)
        {
            var resultado = new List<CategoriaRateioCalculado>();
            var valorBase = Math.Round(valorTotal / entradas.Count, 2, MidpointRounding.AwayFromZero);
            decimal acumulado = 0;

            for (var i = 0; i < entradas.Count; i++)
            {
                var valor = i == entradas.Count - 1
                    ? valorTotal - acumulado
                    : valorBase;

                acumulado += valor;
                resultado.Add(CriarItem(entradas[i].CategoriaId, categoriasDb, valor, valorTotal));
            }

            return resultado;
        }

        private static List<CategoriaRateioCalculado> CalcularRateioPorValor(
            List<CategoriaRateioDto> entradas,
            List<Categoria> categoriasDb,
            decimal valorTotal)
        {
            if (entradas.Any(e => !e.Valor.HasValue || e.Valor.Value <= 0))
                throw new ArgumentException("Todos os valores de rateio devem ser maiores que zero.");

            var soma = entradas.Sum(e => e.Valor!.Value);
            if (soma != valorTotal)
                throw new ArgumentException("A soma dos valores rateados deve ser exatamente igual ao valor total.");

            return entradas
                .Select(e => CriarItem(e.CategoriaId, categoriasDb, e.Valor!.Value, valorTotal))
                .ToList();
        }

        private static List<CategoriaRateioCalculado> CalcularRateioPorPercentual(
            List<CategoriaRateioDto> entradas,
            List<Categoria> categoriasDb,
            decimal valorTotal)
        {
            if (entradas.Any(e => !e.Percentual.HasValue || e.Percentual.Value <= 0))
                throw new ArgumentException("Todos os percentuais de rateio devem ser maiores que zero.");

            var somaPercentual = entradas.Sum(e => e.Percentual!.Value);
            if (somaPercentual != 100)
                throw new ArgumentException("A soma dos percentuais de rateio deve ser exatamente igual a 100.");

            var resultado = new List<CategoriaRateioCalculado>();
            decimal acumulado = 0;

            for (var i = 0; i < entradas.Count; i++)
            {
                var valor = i == entradas.Count - 1
                    ? valorTotal - acumulado
                    : Math.Round(valorTotal * entradas[i].Percentual!.Value / 100, 2, MidpointRounding.AwayFromZero);

                acumulado += valor;
                resultado.Add(CriarItem(entradas[i].CategoriaId, categoriasDb, valor, valorTotal, entradas[i].Percentual!.Value));
            }

            return resultado;
        }

        private static CategoriaRateioCalculado CriarItem(
            int categoriaId,
            List<Categoria> categoriasDb,
            decimal valor,
            decimal valorTotal,
            decimal? percentualInformado = null)
        {
            return new CategoriaRateioCalculado
            {
                Categoria = categoriasDb.First(c => c.Id == categoriaId),
                Valor = valor,
                Percentual = percentualInformado ?? Math.Round(valor / valorTotal * 100, 6, MidpointRounding.AwayFromZero)
            };
        }
    }
}
