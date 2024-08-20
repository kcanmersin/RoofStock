using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity.User;
using Microsoft.AspNetCore.Identity;
using System;

namespace Core.Data.Configuration
{
    internal class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasKey(u => u.Id);

            builder.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");

            builder.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

            builder.Property(u => u.UserName).HasMaxLength(100);
            builder.Property(u => u.NormalizedUserName).HasMaxLength(100);
            builder.Property(u => u.Email).HasMaxLength(100);
            builder.Property(u => u.NormalizedEmail).HasMaxLength(100);

            builder.Property(u => u.Balance)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.EmailConfirmationToken)
                   .IsRequired(false); 

            builder.HasMany<AppUserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            builder.HasMany<AppUserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            builder.HasMany<AppUserToken>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
            builder.HasMany<AppUserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();

            builder.HasMany(u => u.StockHoldings)
                   .WithOne(sh => sh.User)
                   .HasForeignKey(sh => sh.UserId);

            builder.HasMany(u => u.Transactions)
                   .WithOne(t => t.User)
                   .HasForeignKey(t => t.UserId);

            var superadmin = new AppUser
            {
                Id = Guid.Parse("CB94223B-CCB8-4F2F-93D7-0DF96A7F065C"),
                UserName = "superadmin@gmail.com",
                NormalizedUserName = "SUPERADMIN@GMAIL.COM",
                Email = "superadmin@gmail.com",
                NormalizedEmail = "SUPERADMIN@GMAIL.COM",
                PhoneNumber = "+905439999999",
                FirstName = "Can",
                LastName = "Mersin",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ImagePath = "API\\deflogo.jpg",
                Balance = 1000000,
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                EmailConfirmationSentAt = DateTime.UtcNow
            };
            superadmin.PasswordHash = CreatePasswordHash(superadmin, "123456");

            var admin = new AppUser
            {
                Id = Guid.Parse("3AA42229-1C0F-4630-8C1A-DB879ECD0427"),
                UserName = "admin@gmail.com",
                NormalizedUserName = "ADMIN@GMAIL.COM",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                PhoneNumber = "+905439999988",
                FirstName = "Admin",
                LastName = "User",
                PhoneNumberConfirmed = false,
                EmailConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                ImagePath = "API\\deflogo.jpg",
                Balance = 1000000,
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                EmailConfirmationSentAt = DateTime.UtcNow
            };
            admin.PasswordHash = CreatePasswordHash(admin, "19071907");

            builder.HasData(superadmin, admin);
        }

        private string CreatePasswordHash(AppUser user, string password)
        {
            var passwordHasher = new PasswordHasher<AppUser>();
            return passwordHasher.HashPassword(user, password);
        }
    }
}
