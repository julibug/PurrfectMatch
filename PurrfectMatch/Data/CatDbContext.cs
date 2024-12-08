using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Models;

namespace PurrfectMatch.Data
{
    public class CatDbContext : DbContext
    {
        public CatDbContext(DbContextOptions<CatDbContext> options) : base(options)
        {
        }

        public DbSet<Cat> Cats { get; set; }
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }
    }
}
