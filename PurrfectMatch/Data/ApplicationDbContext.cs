using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Models;

namespace PurrfectMatch.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cat> Cats { get; set; } // Tabela dla kotów
    }
}
