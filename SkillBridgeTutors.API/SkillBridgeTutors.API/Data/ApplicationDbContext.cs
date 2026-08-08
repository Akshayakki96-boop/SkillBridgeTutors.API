using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<DemoSlot> DemoSlots => Set<DemoSlot>();
        public DbSet<DemoBooking> DemoBookings => Set<DemoBooking>();
        public DbSet<CallRecord> CallRecords => Set<CallRecord>();
        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DemoBooking>()
                .HasOne(b => b.Lead)
                .WithMany(l => l.DemoBookings)
                .HasForeignKey(b => b.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemoBooking>()
                .HasOne(b => b.DemoSlot)
                .WithMany()
                .HasForeignKey(b => b.DemoSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CallRecord>()
                .HasOne(c => c.Lead)
                .WithMany(l => l.CallRecords)
                .HasForeignKey(c => c.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
