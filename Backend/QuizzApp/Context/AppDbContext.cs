using Microsoft.EntityFrameworkCore;
using QuizzApp.Models;

namespace QuizzApp.Context
{
    // AppDbContext is the main EF Core class that connects to the database
    // It holds all the DbSets (tables) and configures relationships
    public class AppDbContext : DbContext
    {
        // Constructor receives options (connection string etc.) from Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // OnModelCreating is used to configure relationships and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.Property(q => q.Title).IsRequired().HasMaxLength(200);
                entity.Property(q => q.Description).HasMaxLength(1000);

                entity.HasOne(q => q.Category)
                    .WithMany(c => c.Quizzes)
                    .HasForeignKey(q => q.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(q => q.Creator)
                    .WithMany(u => u.Quizzes)
                    .HasForeignKey(q => q.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(q => q.QuestionText).IsRequired().HasMaxLength(500);

                entity.HasOne(q => q.Quiz)
                    .WithMany(qz => qz.Questions)
                    .HasForeignKey(q => q.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Option>(entity =>
            {
                entity.Property(o => o.OptionText).IsRequired().HasMaxLength(300);

                entity.HasOne(o => o.Question)
                    .WithMany(q => q.Options)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuizResult>(entity =>
            {
                // A result belongs to one user (no cascade)
                entity.HasOne(r => r.User)
                    .WithMany(u => u.QuizResults)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Quiz)
                    .WithMany(q => q.QuizResults)
                    .HasForeignKey(r => r.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasOne(ua => ua.User)
                    .WithMany(u => u.UserAnswers)
                    .HasForeignKey(ua => ua.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ua => ua.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(ua => ua.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
                entity.Property(n => n.Type).IsRequired().HasMaxLength(50);

                // UserId is nullable (null = broadcast)
                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
            });
        }
    }
}
