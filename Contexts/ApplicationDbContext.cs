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
        public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
        public DbSet<ContaPagarMovimentacao> ContasPagarMovimentacoes => Set<ContaPagarMovimentacao>();
        public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
        public DbSet<ContaReceberMovimentacao> ContasReceberMovimentacoes => Set<ContaReceberMovimentacao>();
        public DbSet<MovimentacaoCategoria> MovimentacoesCategorias => Set<MovimentacaoCategoria>();
        public DbSet<ContaPagarCategoria> ContasPagarCategorias => Set<ContaPagarCategoria>();
        public DbSet<ContaReceberCategoria> ContasReceberCategorias => Set<ContaReceberCategoria>();

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

            modelBuilder.Entity<MovimentacaoCategoria>()
                .ToTable("MovimentacoesCategorias");

            modelBuilder.Entity<MovimentacaoCategoria>()
                .HasKey(mc => new { mc.MovimentacaoId, mc.CategoriaId });

            modelBuilder.Entity<MovimentacaoCategoria>()
                .HasOne(mc => mc.Movimentacao)
                .WithMany(m => m.CategoriasRateio)
                .HasForeignKey(mc => mc.MovimentacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MovimentacaoCategoria>()
                .HasOne(mc => mc.Categoria)
                .WithMany()
                .HasForeignKey(mc => mc.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

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
            
            modelBuilder.Entity<ContaPagar>()
                .ToTable("ContasPagar");

            modelBuilder.Entity<ContaPagar>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaPagar>()
                .Property(c => c.Status)
                .HasConversion<int>();

            modelBuilder.Entity<ContaPagarMovimentacao>()
                .ToTable("ContasPagarMovimentacoes");

            modelBuilder.Entity<ContaPagarMovimentacao>()
                .HasKey(c => new { c.ContaPagarId, c.MovimentacaoId });

            modelBuilder.Entity<ContaPagarMovimentacao>()
                .HasOne(c => c.ContaPagar)
                .WithMany(c => c.Movimentacoes)
                .HasForeignKey(c => c.ContaPagarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContaPagarMovimentacao>()
                .HasOne(c => c.Movimentacao)
                .WithMany(m => m.ContasPagar)
                .HasForeignKey(c => c.MovimentacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaPagarCategoria>()
                .ToTable("ContasPagarCategorias");

            modelBuilder.Entity<ContaPagarCategoria>()
                .HasKey(c => new { c.ContaPagarId, c.CategoriaId });

            modelBuilder.Entity<ContaPagarCategoria>()
                .HasOne(c => c.ContaPagar)
                .WithMany(c => c.CategoriasRateio)
                .HasForeignKey(c => c.ContaPagarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaPagarCategoria>()
                .HasOne(c => c.Categoria)
                .WithMany()
                .HasForeignKey(c => c.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContaReceber>()
                .ToTable("ContasReceber");

            modelBuilder.Entity<ContaReceber>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaReceber>()
                .Property(c => c.Status)
                .HasConversion<int>();

            modelBuilder.Entity<ContaReceberMovimentacao>()
                .ToTable("ContasReceberMovimentacoes");

            modelBuilder.Entity<ContaReceberMovimentacao>()
                .HasKey(c => new { c.ContaReceberId, c.MovimentacaoId });

            modelBuilder.Entity<ContaReceberMovimentacao>()
                .HasOne(c => c.ContaReceber)
                .WithMany(c => c.Movimentacoes)
                .HasForeignKey(c => c.ContaReceberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContaReceberMovimentacao>()
                .HasOne(c => c.Movimentacao)
                .WithMany(m => m.ContasReceber)
                .HasForeignKey(c => c.MovimentacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaReceberCategoria>()
                .ToTable("ContasReceberCategorias");

            modelBuilder.Entity<ContaReceberCategoria>()
                .HasKey(c => new { c.ContaReceberId, c.CategoriaId });

            modelBuilder.Entity<ContaReceberCategoria>()
                .HasOne(c => c.ContaReceber)
                .WithMany(c => c.CategoriasRateio)
                .HasForeignKey(c => c.ContaReceberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContaReceberCategoria>()
                .HasOne(c => c.Categoria)
                .WithMany()
                .HasForeignKey(c => c.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
