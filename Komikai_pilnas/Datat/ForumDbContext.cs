using Komikai_pilnas.Auth.Model;
using Komikai_pilnas.Datat.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Komikai_pilnas.Datat;

public class ForumDbContext : IdentityDbContext<ForumUser>
{
    private readonly IConfiguration _configuration;
    public DbSet<Comedian> Comedians { get; set; }
    public DbSet<Set> Sets { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public DbSet<Session> Sessions { get; set; }

    public ForumDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString(("PostgreSQL")));
    }
}
