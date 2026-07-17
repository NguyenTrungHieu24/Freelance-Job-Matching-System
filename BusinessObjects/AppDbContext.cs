using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<FreelancerProfile> FreelancerProfiles { get; set; }
        public DbSet<EmployerProfile> EmployerProfiles { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        //public DbSet<Portfolio> Portfolios { get; set; }
        //public DbSet<CV> CVs { get; set; }
        public DbSet<Application> Applications { get; set; }
        //public DbSet<Bookmark> Bookmarks { get; set; }
        //public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FreelancerSkill> FreelancerSkills { get; set; }

        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Job>()
                .Property(j => j.Budget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(a => a.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.FreelancerProfile)
                .WithMany()
                .HasForeignKey(a => a.FreelancerProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FreelancerSkill>()
                .HasKey(fs => new { fs.FreelancerProfileId, fs.SkillId });

            modelBuilder.Entity<JobSkill>()
                .HasKey(js => new { js.JobId, js.SkillId });

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany()
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Account)
                .WithMany(a => a.Notifications)
                .HasForeignKey(n => n.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Report>()
                .HasOne(x => x.Reporter)
                .WithMany()
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(x => x.ReportedUser)
                .WithMany()
                .HasForeignKey(x => x.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(x => x.Resolver)
                .WithMany()
                .HasForeignKey(x => x.ResolvedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();
        }
    }
}
