using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Data

{
    public class BordspelDbContext : IdentityDbContext
    {
        public BordspelDbContext(DbContextOptions<BordspelDbContext> options)
            : base(options)
        {}

        public DbSet<Models.Game> Games { get; set; }
    }
}
