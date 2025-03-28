using ChildApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildApi.Infrastructure.Data
{
    // DbContext cho Child API (quản lý bảng Child và Milestone)
    public class ChildDbContext : DbContext
    {
        public ChildDbContext(DbContextOptions<ChildDbContext> options)
            : base(options)
        {
            // Ensure database is created
            Database.EnsureCreated();
        }

        public DbSet<Child> Children { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        // Nếu cần cấu hình thêm theo Fluent API, bạn có thể override OnModelCreating ở đây.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Child entity
            modelBuilder.Entity<Child>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.Gender).HasMaxLength(10);
            });

            // Configure Milestone entity
            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.MilestoneDate).IsRequired();
                
                // Relationship with Child
                entity.HasOne<Child>()
                    .WithMany()
                    .HasForeignKey(e => e.ChildId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
