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


        // ===== PERSONNEL CONTEXT CONFIGURATION =====
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().HasKey(p => p.Id);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();

        // Basic Information
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.ProjectId).IsRequired();
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Lastname).IsRequired().HasMaxLength(100);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Position).IsRequired().HasMaxLength(150);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Department).IsRequired().HasMaxLength(100);

        // Enum properties
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .Property(p => p.PersonnelType)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .Property(p => p.Status)
            .HasColumnType("varchar(50)")
            .HasConversion<string>();

        // Financial Information
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.MonthlyAmount).HasColumnType("decimal(10,2)");
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.TotalAmount).HasColumnType("decimal(10,2)");
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Discount).HasColumnType("decimal(10,2)");
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Bank).HasMaxLength(50);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.AccountNumber).HasMaxLength(50);

        // Contract Information
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.StartDate).HasColumnType("date");
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.EndDate).HasColumnType("date");

        // Attendance and contact
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.MonthlyAttendanceJson).HasColumnType("json");
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Phone).HasMaxLength(20);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Email).HasMaxLength(150);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.AvatarUrl).HasColumnType("text");

        // Calculated fields
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.WorkedDays).HasDefaultValue(0);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.CompensatoryDays).HasDefaultValue(0);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.UnpaidLeave).HasDefaultValue(0);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Absences).HasDefaultValue(0);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.Sundays).HasDefaultValue(0);
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.TotalDays).HasDefaultValue(0);

        // Audit
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);

        // Ignore computed property
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>().Ignore(p => p.MonthlyAttendance);

        // ===== FOREIGN KEY RELATIONSHIPS =====

        // Personnel belongs to Project
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Personnel_Projects_ProjectId");

        // ===== INDEXES FOR PERFORMANCE =====

        // Index for project queries
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .HasIndex(p => p.ProjectId)
            .HasDatabaseName("IX_Personnel_ProjectId");

        // Index for document number per project (unique)
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .HasIndex(p => new { p.ProjectId, p.DocumentNumber })
            .HasDatabaseName("IX_Personnel_ProjectId_DocumentNumber")
            .IsUnique();

        // Index for active personnel
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .HasIndex(p => new { p.ProjectId, p.Status, p.IsDeleted })
            .HasDatabaseName("IX_Personnel_ProjectId_Status_IsDeleted");

    // Add Personnel DbSet to your AppDbContext class:
    // public DbSet<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel> Personnel { get; set; }
            // ===== NAMING CONVENTION =====
        builder.UseSnakeCaseNamingConvention();
    }
}