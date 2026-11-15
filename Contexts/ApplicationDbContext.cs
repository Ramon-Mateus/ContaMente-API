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
        public DbSet<PluggyItem> PluggyItens => Set<PluggyItem>();
        public DbSet<PluggyConta> PluggyContas => Set<PluggyConta>();
        public DbSet<TransacaoPendente> TransacoesPendentes => Set<TransacaoPendente>();

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

            modelBuilder.Entity<PluggyItem>(e =>
            {
                e.HasIndex(i => i.PluggyItemId).IsUnique();
                e.HasOne(i => i.User).WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(i => i.Contas).WithOne(c => c.Item).HasForeignKey(c => c.ItemId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PluggyConta>(e =>
            {
                e.HasIndex(c => c.PluggyAccountId).IsUnique();
                e.HasOne(c => c.Cartao).WithMany().HasForeignKey(c => c.CartaoId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TransacaoPendente>(e =>
            {
                e.HasIndex(t => new { t.UserId, t.PluggyTransactionId }).IsUnique();
                e.HasIndex(t => new { t.UserId, t.Status });
                e.Property(t => t.Descricao).HasMaxLength(255);
                e.Property(t => t.Status).HasConversion<int>();
                e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(t => t.Movimentacao).WithMany().HasForeignKey(t => t.MovimentacaoId).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
