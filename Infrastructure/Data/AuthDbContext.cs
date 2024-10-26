using BordspelAvond.Core.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Infrastructure.Data
{
    public class AuthDbContext : IdentityDbContext<Player>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        { }
    }
}
