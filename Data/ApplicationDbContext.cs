using Microsoft.EntityFrameworkCore;
using ButceTakipSistemi.Models;

namespace ButceTakipSistemi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Tablolarımızı tanımlıyoruz (DbSet<ModelAdı>)
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}