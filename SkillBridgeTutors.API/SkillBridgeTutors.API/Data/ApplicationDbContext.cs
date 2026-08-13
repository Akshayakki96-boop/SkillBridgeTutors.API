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
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

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
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemoBooking>()
                .HasOne(b => b.Teacher)
                .WithMany(t => t.DemoBookings)
                .HasForeignKey(b => b.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CallRecord>()
                .HasOne(c => c.Lead)
                .WithMany(l => l.CallRecords)
                .HasForeignKey(c => c.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
