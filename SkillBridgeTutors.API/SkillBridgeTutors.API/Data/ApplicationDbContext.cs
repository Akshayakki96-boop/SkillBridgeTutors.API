using Microsoft.EntityFrameworkCore;

namespace SkillBridgeTutors.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSet properties here as you create your models
        // Example: public DbSet<User> Users { get; set; }
    }
}
