using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserTask> Tasks { get; set; }
        public DbSet<RecurrencePattern> RecurrencePatterns { get; set; }
        public DbSet<TaskCollaborator> TaskCollaborators { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure UserTask entity
            modelBuilder.Entity<UserTask>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure RecurrencePattern entity
            modelBuilder.Entity<RecurrencePattern>()
                .HasOne(r => r.Task)
                .WithOne(t => t.RecurrencePattern)
                .HasForeignKey<RecurrencePattern>(r => r.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure recurring task instances relationship
            modelBuilder.Entity<UserTask>()
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.RecurrenceInstances)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure TaskCollaborator entity
            modelBuilder.Entity<TaskCollaborator>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.Collaborators)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<TaskCollaborator>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 