using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
///     Application database context for the BuildTruck Platform
/// </summary>
/// <param name="options">
///     The options for the database context
/// </param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    // ✅ Users DbSet
    public DbSet<User> Users { get; set; }

    /// <summary>
    ///     On configuring the database context
    /// </summary>
    /// <remarks>
    ///     This method is used to configure the database context.
    ///     It also adds the created and updated date interceptor to the database context.
    /// </remarks>
    /// <param name="builder">
    ///     The option builder for the database context
    /// </param>
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    /// <summary>
    ///     On creating the database model
    /// </summary>
    /// <remarks>
    ///     This method is used to create the database model for the application.
    /// </remarks>
    /// <param name="builder">
    ///     The model builder for the database context
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ✅ Users Context Configuration
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        
        // ✅ Configure PersonName Value Object
        builder.Entity<User>().OwnsOne(u => u.Name, n =>
        {
            n.WithOwner().HasForeignKey("Id");
            n.Property(p => p.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(50);
            n.Property(p => p.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(50);
        });

        // ✅ Configure CorporateEmail Value Object
        builder.Entity<User>().OwnsOne(u => u.CorporateEmail, e =>
        {
            e.WithOwner().HasForeignKey("Id");
            e.Property(a => a.Address).HasColumnName("Email").IsRequired().HasMaxLength(100);
        });

        // ✅ Configure UserRole Value Object
        builder.Entity<User>().OwnsOne(u => u.Role, r =>
        {
            r.WithOwner().HasForeignKey("Id");
            r.Property(p => p.Role).HasColumnName("Role").IsRequired().HasMaxLength(20);
        });

        // ✅ Configure ContactInfo Value Object (SIN nested OwnsOne - evita conflictos EF)
        builder.Entity<User>().OwnsOne(u => u.ContactInfo, c =>
        {
            c.WithOwner().HasForeignKey("Id");
            c.Property(p => p.PersonalEmailAddress).HasColumnName("PersonalEmail").HasMaxLength(100);
            c.Property(p => p.Phone).HasColumnName("Phone").HasMaxLength(20);
        });

        // ✅ Other User properties
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Entity<User>().Property(u => u.ProfileImageUrl).HasMaxLength(500);
        builder.Entity<User>().Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
        builder.Entity<User>().Property(u => u.LastLogin);

        // ✅ Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}