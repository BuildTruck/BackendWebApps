using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Projects.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using BuildTruckBack.Incidents.Domain.Aggregates;
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


    // ✅ Machinery DbSet
    public DbSet<Machinery.Domain.Model.Aggregates.Machinery> Machinery { get; set; }
    
    public DbSet<BuildTruckBack.Configurations.Domain.Model.Aggregates.Configuration> Configurations { get; set; }

    // ✅ Personnel DbSet
    public DbSet<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel> Personnel { get; set; }

    // ✅ Materials DbSets
    public DbSet<BuildTruckBack.Materials.Domain.Model.Aggregates.Material> Materials { get; set; }
    public DbSet<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry> MaterialEntries { get; set; }
    public DbSet<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage> MaterialUsages { get; set; }
    public DbSet<Incident> Incidents { get; set; } // Add Incidents entity



    /// <summary>
    ///     On configuring  the database context
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
        
        // ===== CONFIGURATIONS CONTEXT CONFIGURATION =====
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().HasKey(c => c.Id);
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().Property(c => c.UserId).IsRequired();
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().Property(c => c.Theme).IsRequired().HasMaxLength(10);
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().Property(c => c.Plan).IsRequired().HasMaxLength(20);
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().Property(c => c.NotificationsEnable).IsRequired();
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>().Property(c => c.EmailNotifications).IsRequired();

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
        
        // Configuration belongs to User
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Configurations_Users_UserId");

        // ===== INDEXES FOR PERFORMANCE =====
        
        // Index for manager queries
        builder.Entity<Project>()
            .HasIndex(p => p.ManagerId)
            .HasDatabaseName("IX_Projects_ManagerId");

        // Index for supervisor queries  
        builder.Entity<Project>()
            .HasIndex(p => p.SupervisorId)
            .HasDatabaseName("IX_Projects_SupervisorId");
        
        // Index for user queries (Configurations)
        builder.Entity<Configurations.Domain.Model.Aggregates.Configuration>()
            .HasIndex(c => c.UserId)
            .HasDatabaseName("IX_Configurations_UserId")
            .IsUnique();

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

        // Personnel belongs to Project
        builder.Entity<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Personnel_Projects_ProjectId");

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
    
    
    // Machinery Configuration
   
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().HasKey(m => m.Id);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.ProjectId).IsRequired();
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.Name).IsRequired().HasMaxLength(100);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.LicensePlate).IsRequired().HasMaxLength(20);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.MachineryType).IsRequired().HasMaxLength(50);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.Status)
        .IsRequired()
        .HasConversion<string>()  // Store enum as string
        .HasMaxLength(20);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.Provider).IsRequired().HasMaxLength(100);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.Description).HasMaxLength(500);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.PersonnelId);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.ImageUrl).HasMaxLength(500);
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.CreatedAt).IsRequired();
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.UpdatedAt).IsRequired();
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>().Property(m => m.RegisterDate).IsRequired();

    // Machinery-Project Relationship
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>()
        .HasOne<Project>()
        .WithMany()
        .HasForeignKey(m => m.ProjectId)
        .OnDelete(DeleteBehavior.Cascade);  // Delete machinery when project is deleted

    // Ensure LicensePlate is unique per project
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>()
        .HasIndex(m => new { m.ProjectId, m.LicensePlate })
        .IsUnique();

    // Index for status filtering
    builder.Entity<Machinery.Domain.Model.Aggregates.Machinery>()
        .HasIndex(m => m.Status)
        .HasDatabaseName("IX_Machinery_Status");
    
    // public DbSet<BuildTruckBack.Personnel.Domain.Model.Aggregates.Personnel> Personnel { get; set; }
            // ===== NAMING CONVENTION =====

        // ===== MATERIALS CONTEXT CONFIGURATION =====
        
        // Material Configuration
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().HasKey(m => m.Id);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().Property(m => m.ProjectId).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().Property(m => m.Provider).HasMaxLength(100);

        // Configure MaterialName Value Object
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().OwnsOne(m => m.Name, n =>
        {
            n.WithOwner().HasForeignKey("Id");
            n.Property(mn => mn.Value).HasColumnName("Name").IsRequired().HasMaxLength(100);
        });

        // Configure MaterialType Value Object
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().OwnsOne(m => m.Type, t =>
        {
            t.WithOwner().HasForeignKey("Id");
            t.Property(mt => mt.Value).HasColumnName("Type").IsRequired().HasMaxLength(50);
        });

        // Configure MaterialUnit Value Object
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().OwnsOne(m => m.Unit, u =>
        {
            u.WithOwner().HasForeignKey("Id");
            u.Property(mu => mu.Value).HasColumnName("Unit").IsRequired().HasMaxLength(20);
        });

        // Configure MaterialQuantity Value Objects
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().OwnsOne(m => m.MinimumStock, ms =>
        {
            ms.WithOwner().HasForeignKey("Id");
            ms.Property(mq => mq.Value).HasColumnName("MinimumStock").IsRequired().HasColumnType("decimal(10,2)");
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().OwnsOne(m => m.Stock, s =>
        {
            s.WithOwner().HasForeignKey("Id");
            s.Property(mq => mq.Value).HasColumnName("Stock").IsRequired().HasColumnType("decimal(10,2)").HasDefaultValue(0);
        });

        // Configure MaterialCost Value Object
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>().OwnsOne(m => m.Price, p =>
        {
            p.WithOwner().HasForeignKey("Id");
            p.Property(mc => mc.Value).HasColumnName("Price").IsRequired().HasColumnType("decimal(10,2)").HasDefaultValue(0);
        });

        // MaterialEntry Configuration
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().HasKey(me => me.Id);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.ProjectId).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.MaterialId).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.Date).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.Provider).HasMaxLength(100);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.Ruc).HasMaxLength(11);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.DocumentNumber).HasMaxLength(50);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().Property(me => me.Observations).HasMaxLength(500);

        // Configure MaterialEntry Value Objects
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().OwnsOne(me => me.Quantity, q =>
        {
            q.WithOwner().HasForeignKey("Id");
            q.Property(mq => mq.Value).HasColumnName("Quantity").IsRequired().HasColumnType("decimal(10,2)");
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().OwnsOne(me => me.Unit, u =>
        {
            u.WithOwner().HasForeignKey("Id");
            u.Property(mu => mu.Value).HasColumnName("Unit").IsRequired().HasMaxLength(20);
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().OwnsOne(me => me.Payment, p =>
        {
            p.WithOwner().HasForeignKey("Id");
            p.Property(mp => mp.Value).HasColumnName("Payment").IsRequired().HasMaxLength(50);
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().OwnsOne(me => me.DocumentType, dt =>
        {
            dt.WithOwner().HasForeignKey("Id");
            dt.Property(mdt => mdt.Value).HasColumnName("DocumentType").IsRequired().HasMaxLength(50);
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().OwnsOne(me => me.UnitCost, uc =>
        {
            uc.WithOwner().HasForeignKey("Id");
            uc.Property(muc => muc.Value).HasColumnName("UnitCost").IsRequired().HasColumnType("decimal(10,2)");
        });

        // TotalCost es una propiedad calculada, no se persiste en BD
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .Ignore(me => me.TotalCost);

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>().OwnsOne(me => me.Status, s =>
        {
            s.WithOwner().HasForeignKey("Id");
            s.Property(ms => ms.Value).HasColumnName("Status").IsRequired().HasMaxLength(20);
        });

        // MaterialUsage Configuration
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().HasKey(mu => mu.Id);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.ProjectId).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.MaterialId).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.Date).IsRequired();
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.Area).HasMaxLength(100);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.Worker).HasMaxLength(100);
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().Property(mu => mu.Observations).HasMaxLength(500);

        // Configure MaterialUsage Value Objects
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().OwnsOne(mu => mu.Quantity, q =>
        {
            q.WithOwner().HasForeignKey("Id");
            q.Property(mq => mq.Value).HasColumnName("Quantity").IsRequired().HasColumnType("decimal(10,2)");
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().OwnsOne(mu => mu.UsageType, ut =>
        {
            ut.WithOwner().HasForeignKey("Id");
            ut.Property(mut => mut.Value).HasColumnName("UsageType").IsRequired().HasMaxLength(50);
        });

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>().OwnsOne(mu => mu.Status, s =>
        {
            s.WithOwner().HasForeignKey("Id");
            s.Property(ms => ms.Value).HasColumnName("Status").IsRequired().HasMaxLength(20);
        });

        // ===== MATERIALS FOREIGN KEY RELATIONSHIPS =====

        // Material belongs to Project
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Materials_Projects_ProjectId");

        // MaterialEntry belongs to Project and Material
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(me => me.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialEntries_Projects_ProjectId");

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .HasOne<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>()
            .WithMany()
            .HasForeignKey(me => me.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialEntries_Materials_MaterialId");

        // MaterialUsage belongs to Project and Material
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>()
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(mu => mu.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialUsages_Projects_ProjectId");

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>()
            .HasOne<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>()
            .WithMany()
            .HasForeignKey(mu => mu.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialUsages_Materials_MaterialId");

        // ===== MATERIALS INDEXES FOR PERFORMANCE =====

        // Material indexes
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>()
            .HasIndex(m => m.ProjectId)
            .HasDatabaseName("IX_Materials_ProjectId");

        // MaterialEntry indexes
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .HasIndex(me => me.ProjectId)
            .HasDatabaseName("IX_MaterialEntries_ProjectId");

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .HasIndex(me => me.MaterialId)
            .HasDatabaseName("IX_MaterialEntries_MaterialId");

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .HasIndex(me => me.Date)
            .HasDatabaseName("IX_MaterialEntries_Date");

        // MaterialUsage indexes
        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>()
            .HasIndex(mu => mu.ProjectId)
            .HasDatabaseName("IX_MaterialUsages_ProjectId");

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>()
            .HasIndex(mu => mu.MaterialId)
            .HasDatabaseName("IX_MaterialUsages_MaterialId");

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage>()
            .HasIndex(mu => mu.Date)
            .HasDatabaseName("IX_MaterialUsages_Date");
        
        // Incident configuration
        // ===== INCIDENTS CONTEXT CONFIGURATION =====
        builder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).IsRequired(false);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255).HasDefaultValue("");
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000).HasDefaultValue("");
            entity.Property(e => e.IncidentType).IsRequired().HasMaxLength(50).HasDefaultValue("");
            entity.Property(e => e.Severity)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>()
                .HasDefaultValue("MEDIO");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>()
                .HasDefaultValue("REPORTADO");
            entity.Property(e => e.Location).IsRequired().HasMaxLength(255).HasDefaultValue("");
            entity.Property(e => e.ReportedBy).IsRequired(false).HasMaxLength(50);
            entity.Property(e => e.AssignedTo).IsRequired(false).HasMaxLength(50);
            entity.Property(e => e.OccurredAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ResolvedAt).IsRequired(false);
            entity.Property(e => e.Image).IsRequired(false).HasMaxLength(500);
            entity.Property(e => e.Notes).IsRequired().HasMaxLength(1000).HasDefaultValue("");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RegisterDate).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relación con Project
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Incidents_Projects_ProjectId");

            // Índice para consultas por ProjectId
            entity.HasIndex(e => e.ProjectId)
                .HasDatabaseName("IX_Incidents_ProjectId");
        });



        // ===== NAMING CONVENTION =====

        builder.UseSnakeCaseNamingConvention();
    }
}