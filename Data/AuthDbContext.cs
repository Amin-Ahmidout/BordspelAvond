using BordspelAvond.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BordspelAvond.Data
{
    public class AuthDbContext : IdentityDbContext<Player>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        { }
    }
}
