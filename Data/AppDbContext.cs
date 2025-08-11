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
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveAttachment> LeaveAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ User Configuration
            modelBuilder.Entity<User>()
                .HasIndex(s => s.Email)
                .IsUnique();

            // ✅ Staff Configuration
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.Property(e => e.EmployeeCode).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");

                // Relationship with User
                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ Project Configuration
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

            // ✅ Client Configuration
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                entity.Property(c => c.ContactEmail).IsRequired().HasMaxLength(255);
                entity.HasIndex(c => c.ContactEmail).IsUnique();
            });

            // ✅ ProjectStaffAssignment Configuration
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

            // ✅ Task Configuration
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

            // Configure Tags as JSON column
            modelBuilder.Entity<Models.Entities.Task>()
                .Property(t => t.Tags)
                .HasConversion(
                    tags => System.Text.Json.JsonSerializer.Serialize(tags, (JsonSerializerOptions)null),
                    tags => System.Text.Json.JsonSerializer.Deserialize<List<string>>(tags, (JsonSerializerOptions)null) ?? new List<string>());

            // ✅ TaskComment Configuration
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

            // ✅ Notification Configuration
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ TaskAttachment Configuration
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

            // ✅ LEAVE ENTITY CONFIGURATION
            modelBuilder.Entity<Leave>(entity =>
            {
                entity.HasKey(l => l.Id);

                // ✅ Property configurations
                entity.Property(l => l.UserId).IsRequired(); // ✅ Changed from StaffId to UserId
                entity.Property(l => l.Reason).IsRequired().HasMaxLength(1000);
                entity.Property(l => l.ApprovalComments).HasMaxLength(1000);
                entity.Property(l => l.LeaveType).HasConversion<int>(); // ✅ Convert enum to int
                entity.Property(l => l.Status).HasConversion<int>(); // ✅ Convert enum to int
                entity.Property(l => l.TotalDays).IsRequired();
                entity.Property(l => l.StartDate).IsRequired();
                entity.Property(l => l.EndDate).IsRequired();
                entity.Property(l => l.IsActive).HasDefaultValue(true);
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(l => l.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // ✅ Relationships
                entity.HasOne(l => l.User) // ✅ Changed from Staff to User
                      .WithMany()
                      .HasForeignKey(l => l.UserId) // ✅ Changed from StaffId to UserId
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(l => l.ApprovedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(l => l.Attachments)
                      .WithOne(a => a.Leave)
                      .HasForeignKey(a => a.LeaveId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ✅ Single column indexes for performance
                entity.HasIndex(l => l.UserId)
                      .HasDatabaseName("IX_Leaves_UserId");
                entity.HasIndex(l => l.Status)
                      .HasDatabaseName("IX_Leaves_Status");
                entity.HasIndex(l => l.LeaveType)
                      .HasDatabaseName("IX_Leaves_LeaveType");
                entity.HasIndex(l => l.StartDate)
                      .HasDatabaseName("IX_Leaves_StartDate");
                entity.HasIndex(l => l.EndDate)
                      .HasDatabaseName("IX_Leaves_EndDate");
                entity.HasIndex(l => l.IsActive)
                      .HasDatabaseName("IX_Leaves_IsActive");
                entity.HasIndex(l => l.ApprovedById)
                      .HasDatabaseName("IX_Leaves_ApprovedById");

                // ✅ Composite indexes for common query patterns
                entity.HasIndex(l => new { l.UserId, l.Status, l.IsActive })
                      .HasDatabaseName("IX_Leaves_UserId_Status_IsActive");
                entity.HasIndex(l => new { l.Status, l.CreatedAt })
                      .HasDatabaseName("IX_Leaves_Status_CreatedAt");
                entity.HasIndex(l => new { l.LeaveType, l.StartDate, l.Status })
                      .HasDatabaseName("IX_Leaves_LeaveType_StartDate_Status");
            });

            // ✅ LEAVE ATTACHMENT ENTITY CONFIGURATION
            modelBuilder.Entity<LeaveAttachment>(entity =>
            {
                entity.HasKey(la => la.Id);

                // ✅ Property configurations
                entity.Property(la => la.LeaveId).IsRequired();
                entity.Property(la => la.FileName).IsRequired().HasMaxLength(255);
                entity.Property(la => la.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(la => la.FileType).HasMaxLength(100);
                entity.Property(la => la.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(la => la.FileUrl).IsRequired().HasMaxLength(500);
                entity.Property(la => la.UploadedById).IsRequired();
                entity.Property(la => la.IsActive).HasDefaultValue(true);
                entity.Property(la => la.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // ✅ Relationships
                entity.HasOne(la => la.Leave)
                      .WithMany(l => l.Attachments)
                      .HasForeignKey(la => la.LeaveId)
                      .OnDelete(DeleteBehavior.Cascade); // ✅ Delete attachments when leave is deleted

                entity.HasOne(la => la.UploadedBy)
                      .WithMany()
                      .HasForeignKey(la => la.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict); // ✅ Prevent deleting users with attachments

                // ✅ Indexes for performance
                entity.HasIndex(la => la.LeaveId)
                      .HasDatabaseName("IX_LeaveAttachments_LeaveId");
                entity.HasIndex(la => la.UploadedById)
                      .HasDatabaseName("IX_LeaveAttachments_UploadedById");
                entity.HasIndex(la => la.IsActive)
                      .HasDatabaseName("IX_LeaveAttachments_IsActive");
                entity.HasIndex(la => new { la.LeaveId, la.IsActive })
                      .HasDatabaseName("IX_LeaveAttachments_LeaveId_IsActive");
            });

            // ✅ Performance Indexes for existing entities
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
                .HasIndex(ta => ta.TaskId);
        }
    }
}
