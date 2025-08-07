using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;

namespace TeamDesk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ProjectStaffAssignment> ProjectStaffAssignments { get; set; }
        public DbSet<Models.Entities.Task> Tasks { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.Property(e => e.EmployeeCode).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)"); // ✅ Added this line

                // Relationship with User
                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Budget).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Status).HasConversion<int>();
                entity.Property(p => p.Priority).HasConversion<int>();

                entity.HasOne(p => p.Client)
                      .WithMany(c => c.Projects)
                      .HasForeignKey(p => p.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.StaffAssignments)
                      .WithOne(psa => psa.Project)
                      .HasForeignKey(psa => psa.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                entity.Property(c => c.ContactEmail).IsRequired().HasMaxLength(255);
                entity.HasIndex(c => c.ContactEmail).IsUnique();
            });

            modelBuilder.Entity<ProjectStaffAssignment>(entity =>
            {
                entity.HasKey(psa => psa.Id);
                entity.Property(psa => psa.Role).HasConversion<int>();
                entity.Property(psa => psa.AllocationPercentage).HasColumnType("decimal(5,2)");

                entity.HasOne(psa => psa.Staff)
                      .WithMany()
                      .HasForeignKey(psa => psa.StaffId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(psa => new { psa.ProjectId, psa.StaffId, psa.IsActive })
                      .IsUnique()
                      .HasFilter("[IsActive] = 1");
            });

            modelBuilder.Entity<Models.Entities.Task>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Entities.Task>()
                .HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<Models.Entities.Task>()
            //    .HasOne(t => t.CreatedBy)
            //    .WithMany()
            //    .HasForeignKey(t => t.CreatedById)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Configure Tags as JSON column
            modelBuilder.Entity<Models.Entities.Task>()
                .Property(t => t.Tags)
                .HasConversion(
                    tags => System.Text.Json.JsonSerializer.Serialize(tags, (JsonSerializerOptions)null),
                    tags => System.Text.Json.JsonSerializer.Deserialize<List<string>>(tags, (JsonSerializerOptions)null) ?? new List<string>());

            // TaskComment entity configuration
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification entity configuration
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for better performance
            modelBuilder.Entity<Models.Entities.Task>()
                .HasIndex(t => t.ProjectId);

            modelBuilder.Entity<Models.Entities.Task>()
                .HasIndex(t => t.AssignedToId);

            modelBuilder.Entity<Models.Entities.Task>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<Models.Entities.Task>()
                .HasIndex(t => t.DueDate);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });
            modelBuilder.Entity<TaskAttachment>()
            .HasOne(ta => ta.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAttachment>()
                .HasOne(ta => ta.UploadedBy)
                .WithMany()
                .HasForeignKey(ta => ta.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for performance
            modelBuilder.Entity<TaskAttachment>()
                .HasIndex(ta => ta.TaskId);
        }

    }

}
