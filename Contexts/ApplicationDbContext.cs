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
        public DbSet<Responsavel> Responsaveis => Set<Responsavel>();
        public DbSet<Cartao> Cartoes => Set<Cartao>();
        public DbSet<UserConfiguration> UserConfigurations => Set<UserConfiguration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Responsavel>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Movimentacao>()
                .HasOne(g => g.Categoria)
                .WithMany(c => c.Movimentacoes)
                .HasForeignKey(g => g.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.TipoPagamento)
                .HasConversion<int>();

            modelBuilder.Entity<Cartao>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<UserConfiguration>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
