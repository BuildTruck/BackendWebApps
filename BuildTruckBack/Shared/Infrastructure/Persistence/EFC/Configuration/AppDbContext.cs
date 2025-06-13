using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Projects.Domain.Model.Aggregates;
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
    
    // ✅ Projects DbSet
    public DbSet<Project> Projects { get; set; }

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

        // ===== USERS CONTEXT CONFIGURATION =====
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        
        // Configure PersonName Value Object
        builder.Entity<User>().OwnsOne(u => u.Name, n =>
        {
            n.WithOwner().HasForeignKey("Id");
            n.Property(p => p.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(50);
            n.Property(p => p.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(50);
        });

        // Configure CorporateEmail Value Object
        builder.Entity<User>().OwnsOne(u => u.CorporateEmail, e =>
        {
            e.WithOwner().HasForeignKey("Id");
            e.Property(a => a.Address).HasColumnName("Email").IsRequired().HasMaxLength(100);
        });

        // Configure UserRole Value Object
        builder.Entity<User>().OwnsOne(u => u.Role, r =>
        {
            r.WithOwner().HasForeignKey("Id");
            r.Property(p => p.Role).HasColumnName("Role").IsRequired().HasMaxLength(20);
        });

        // Configure ContactInfo Value Object
        builder.Entity<User>().OwnsOne(u => u.ContactInfo, c =>
        {
            c.WithOwner().HasForeignKey("Id");
            c.Property(p => p.PersonalEmailAddress).HasColumnName("PersonalEmail").HasMaxLength(100);
            c.Property(p => p.Phone).HasColumnName("Phone").HasMaxLength(20);
        });

        // Other User properties
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Entity<User>().Property(u => u.ProfileImageUrl).HasMaxLength(500);
        builder.Entity<User>().Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
        builder.Entity<User>().Property(u => u.LastLogin);

        // ===== PROJECTS CONTEXT CONFIGURATION =====
        builder.Entity<Project>().HasKey(p => p.Id);
        builder.Entity<Project>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();

        // Configure ProjectName Value Object
        builder.Entity<Project>().OwnsOne(p => p.Name, n =>
        {
            n.WithOwner().HasForeignKey("Id");
            n.Property(pn => pn.Name).HasColumnName("Name").IsRequired().HasMaxLength(100);
        });

        // Configure ProjectDescription Value Object
        builder.Entity<Project>().OwnsOne(p => p.Description, d =>
        {
            d.WithOwner().HasForeignKey("Id");
            d.Property(pd => pd.Description).HasColumnName("Description").IsRequired().HasMaxLength(1000);
        });

        // Configure ProjectLocation Value Object
        builder.Entity<Project>().OwnsOne(p => p.Location, l =>
        {
            l.WithOwner().HasForeignKey("Id");
            l.Property(pl => pl.Location).HasColumnName("Location").IsRequired().HasMaxLength(200);
        });

        // Configure ProjectState Value Object
        builder.Entity<Project>().OwnsOne(p => p.State, s =>
        {
            s.WithOwner().HasForeignKey("Id");
            s.Property(ps => ps.State).HasColumnName("State").IsRequired().HasMaxLength(20);
        });

        // Configure ProjectCoordinates Value Object
        builder.Entity<Project>().OwnsOne(p => p.Coordinates, c =>
        {
            c.WithOwner().HasForeignKey("Id");
            c.Property(pc => pc.Latitude).HasColumnName("Latitude").HasPrecision(10, 8);
            c.Property(pc => pc.Longitude).HasColumnName("Longitude").HasPrecision(11, 8);
        });

        // Other Project properties
        builder.Entity<Project>().Property(p => p.ManagerId).IsRequired();
        builder.Entity<Project>().Property(p => p.SupervisorId);
        builder.Entity<Project>().Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Entity<Project>().Property(p => p.StartDate);

        // ===== FOREIGN KEY RELATIONSHIPS =====
        
        // Manager relationship: 1 Manager -> N Projects
        builder.Entity<Project>()
            .HasOne<User>() // No navigation property, solo FK
            .WithMany() // No navigation property en User
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict) // No borrar User si tiene Projects
            .HasConstraintName("FK_Projects_Users_ManagerId");

        // Supervisor relationship: 1 Supervisor -> 1 Project (at a time)
        builder.Entity<Project>()
            .HasOne<User>() // No navigation property, solo FK
            .WithMany() // No navigation property en User  
            .HasForeignKey(p => p.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull) // Si se borra Supervisor, project.SupervisorId = null
            .HasConstraintName("FK_Projects_Users_SupervisorId");

        // ===== INDEXES FOR PERFORMANCE =====
        
        // Index for manager queries
        builder.Entity<Project>()
            .HasIndex(p => p.ManagerId)
            .HasDatabaseName("IX_Projects_ManagerId");

        // Index for supervisor queries  
        builder.Entity<Project>()
            .HasIndex(p => p.SupervisorId)
            .HasDatabaseName("IX_Projects_SupervisorId");

        // Business constraint: Solo un supervisor por proyecto activo
        // Note: Since State is a Value Object, we use a simpler approach
        builder.Entity<Project>()
            .HasIndex(p => p.SupervisorId)
            .HasDatabaseName("IX_Projects_SupervisorId_Business")
            .HasFilter("supervisor_id IS NOT NULL");

        // ===== NAMING CONVENTION =====
        builder.UseSnakeCaseNamingConvention();
    }
}