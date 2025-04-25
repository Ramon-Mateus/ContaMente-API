using ContaMente.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }
        public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Parcela> Parcelas => Set<Parcela>();
        public DbSet<Recorrencia> Recorrencias => Set<Recorrencia>();
        public DbSet<TipoPagamento> TiposPagamento => Set<TipoPagamento>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Movimentacao>()
                .HasOne(g => g.Categoria)
                .WithMany(c => c.Movimentacoes)
                .HasForeignKey(g => g.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
