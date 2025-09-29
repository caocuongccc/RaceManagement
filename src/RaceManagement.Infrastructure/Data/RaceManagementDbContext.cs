using Microsoft.EntityFrameworkCore;
using RaceManagement.Core.Entities;
using GoogleCredential = RaceManagement.Core.Entities.GoogleCredential;

namespace RaceManagement.Infrastructure.Data
{
    public class RaceManagementDbContext : DbContext
    {
        public RaceManagementDbContext(DbContextOptions<RaceManagementDbContext> options) : base(options)
        {
        }

        public DbSet<Race> Races { get; set; }
        public DbSet<RaceDistance> RaceDistances { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<RaceShirtType> RaceShirtTypes { get; set; }     // NEW
        public DbSet<GoogleCredential> GoogleCredentials { get; set; }
        public DbSet<GoogleSheetConfig> GoogleSheetConfigs { get; set; }
        public DbSet<EmailQueue> EmailQueues { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Race Configuration
            modelBuilder.Entity<Race>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EmailPassword).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SheetId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PaymentSheetId).HasMaxLength(255);
                entity.Property(e => e.GoogleCredentialPath).HasMaxLength(500);  // NEW
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.HasShirtSale).HasDefaultValue(false);
                entity.Property(e => e.BankName).HasMaxLength(100);
                entity.Property(e => e.BankAccountNo).HasMaxLength(50);
                entity.Property(e => e.BankAccountHolder).HasMaxLength(255);
                // Add relationship to GoogleSheetConfig
                entity.HasOne(e => e.SheetConfig)
                    .WithMany(e => e.Races)
                    .HasForeignKey(e => e.SheetConfigId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when config deleted
                            entity.HasIndex(e => e.SheetConfigId);
                            entity.HasIndex(e => e.SheetId).IsUnique();
                        });

            // RaceDistance Configuration
            modelBuilder.Entity<RaceDistance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Distance).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.BibPrefix).HasMaxLength(10);

                entity.HasOne(e => e.Race)
                    .WithMany(e => e.Distances)
                    .HasForeignKey(e => e.RaceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Registration Configuration
            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.BibName).HasMaxLength(255);               // NEW
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.RawBirthInput).HasMaxLength(100);         // NEW
                entity.Property(e => e.ShirtCategory).HasMaxLength(50);          // NEW
                entity.Property(e => e.ShirtSize).HasMaxLength(10);
                entity.Property(e => e.ShirtType).HasMaxLength(50);              // NEW
                entity.Property(e => e.EmergencyContact).HasMaxLength(255);
                entity.Property(e => e.PaymentStatus).HasConversion<string>();
                entity.Property(e => e.Gender).HasConversion<string>();
                entity.Property(e => e.BibNumber).HasMaxLength(20);
                entity.Property(e => e.QRCodePath).HasMaxLength(500);
                entity.Property(e => e.TransactionReference).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0);

                entity.HasIndex(e => e.TransactionReference).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => new { e.RaceId, e.Email });
                entity.HasIndex(e => new { e.RaceId, e.DistanceId });
                    entity.Property(e => e.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0);

                entity.HasOne(e => e.Race)
                    .WithMany(e => e.Registrations)
                    .HasForeignKey(e => e.RaceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Distance)
                    .WithMany(e => e.Registrations)
                    .HasForeignKey(e => e.DistanceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionId).HasMaxLength(255);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.BankCode).HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.HasOne(e => e.Registration)
                    .WithMany(e => e.Payments)
                    .HasForeignKey(e => e.RegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // EmailLog Configuration
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmailType).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();

                entity.HasOne(e => e.Registration)
                    .WithMany(e => e.EmailLogs)
                    .HasForeignKey(e => e.RegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add new properties
                entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.RecipientName).HasMaxLength(255);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ProcessingTime).HasConversion<long>();
                entity.Property(e => e.MessageId).HasMaxLength(255);
                entity.Property(e => e.TemplateName).HasMaxLength(100);

                // Add indexes
                entity.HasIndex(e => e.MessageId);
                entity.HasIndex(e => e.TemplateName);

                // Relationship with EmailQueue
                entity.HasOne(e => e.EmailQueue)
                    .WithMany()
                    .HasForeignKey(e => e.EmailQueueId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // RaceShirtType Configuration - NEW
            modelBuilder.Entity<RaceShirtType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ShirtCategory).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ShirtType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AvailableSizes).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.RaceId);
                entity.HasIndex(e => new { e.RaceId, e.ShirtCategory, e.ShirtType });
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.Race)
                    .WithMany(e => e.ShirtTypes)
                    .HasForeignKey(e => e.RaceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GoogleCredential Configuration
            modelBuilder.Entity<GoogleCredential>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.ServiceAccountEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CredentialFilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                // Unique constraints
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.ServiceAccountEmail).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // GoogleSheetConfig Configuration
            modelBuilder.Entity<GoogleSheetConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SpreadsheetId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SheetName).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.HeaderRowIndex).HasDefaultValue(1);
                entity.Property(e => e.DataStartRowIndex).HasDefaultValue(2);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Foreign key to GoogleCredentials
                entity.HasOne(e => e.Credential)
                    .WithMany(e => e.SheetConfigs)
                    .HasForeignKey(e => e.CredentialId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete

                // Indexes
                entity.HasIndex(e => e.CredentialId);
                entity.HasIndex(e => e.SpreadsheetId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.Name, e.CredentialId }).IsUnique();
            });

            // EmailQueue Configuration
            modelBuilder.Entity<EmailQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.RecipientName).HasMaxLength(255);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
                entity.Property(e => e.EmailType).HasConversion<string>();
                entity.Property(e => e.Priority).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.MessageId).HasMaxLength(255);
                entity.Property(e => e.Metadata).HasMaxLength(4000);

                // Indexes for performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.EmailType);
                entity.HasIndex(e => e.ScheduledAt);
                entity.HasIndex(e => new { e.Status, e.ScheduledAt });
                entity.HasIndex(e => new { e.RegistrationId, e.EmailType });

                // Foreign key
                entity.HasOne(e => e.Registration)
                    .WithMany()
                    .HasForeignKey(e => e.RegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Add some default data seeding if needed
            SeedDefaultData(modelBuilder);
        }

        private void SeedDefaultData(ModelBuilder modelBuilder)
        {
            // Seed default shirt types for demo (optional)
            // This can be removed in production
            /*
            modelBuilder.Entity<RaceShirtType>().HasData(
                new RaceShirtType { Id = 1, RaceId = 1, ShirtCategory = "Nam", ShirtType = "T-Shirt", AvailableSizes = "S,M,L,XL,XXL", IsActive = true },
                new RaceShirtType { Id = 2, RaceId = 1, ShirtCategory = "Nữ", ShirtType = "T-Shirt", AvailableSizes = "XS,S,M,L,XL", IsActive = true },
                new RaceShirtType { Id = 3, RaceId = 1, ShirtCategory = "Trẻ em", ShirtType = "T-Shirt", AvailableSizes = "KID-10,KID-12,KID-15", IsActive = true }
            );
            */
        }

        // Override SaveChanges to auto-update UpdatedAt
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.Now;
                        entry.Entity.UpdatedAt = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.Now;
                        entry.Property(e => e.CreatedAt).IsModified = false; // Don't update CreatedAt
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
