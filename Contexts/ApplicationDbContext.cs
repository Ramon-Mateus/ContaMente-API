using ContaMente.Models;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }
        public DbSet<Gasto> Gastos => Set<Gasto>();
        public DbSet<Categoria> Categorias => Set<Categoria>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
