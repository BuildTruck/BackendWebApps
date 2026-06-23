using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using BuildTruckBack.Notifications.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Infrastructure.Persistence.EFC.Configuration;

namespace BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
///     Application database context for the BuildTruck Platform
/// </summary>
/// <param name="options">
///     The options for the database context
/// </param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    // ✅ Materials DbSets
    public DbSet<BuildTruckBack.Materials.Domain.Model.Aggregates.Material> Materials { get; set; }
    public DbSet<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry> MaterialEntries { get; set; }
    public DbSet<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialUsage> MaterialUsages { get; set; }
    // ✅ Stats DbSets
    public DbSet<ManagerStats> ManagerStats { get; set; }
    public DbSet<StatsHistory> StatsHistory { get; set; }
    
    // ✅ Notifications DbSets
    public DbSet<BuildTruckBack.Notifications.Domain.Model.Aggregates.Notification> Notifications { get; set; }
    public DbSet<BuildTruckBack.Notifications.Domain.Model.Aggregates.NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<BuildTruckBack.Notifications.Domain.Model.Aggregates.NotificationDelivery> NotificationDeliveries { get; set; }
    
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

        // ===== IGNORE VALUE OBJECTS =====
        // Ignorar todos los value objects para que EF no los trate como entidades
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.DeliveryStatus>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationType>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationChannel>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationContext>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationPriority>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationStatus>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationScope>();
        builder.Ignore<BuildTruckBack.Notifications.Domain.Model.ValueObjects.UserRole>();
        
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

       

        // ===== MATERIALS FOREIGN KEY RELATIONSHIPS =====

        // FK Materials->Projects exists in MySQL but not registered in EF (owned by ProjectServiceDbContext)

        // MaterialEntry belongs to Material

        builder.Entity<BuildTruckBack.Materials.Domain.Model.Aggregates.MaterialEntry>()
            .HasOne<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>()
            .WithMany()
            .HasForeignKey(me => me.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialEntries_Materials_MaterialId");

        // FK MaterialUsages->Projects exists in MySQL but not registered in EF (owned by ProjectServiceDbContext)

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
        //Stats tables
        builder.ApplyConfiguration(new ManagerStatsConfiguration());
        builder.ApplyConfiguration(new StatsHistoryConfiguration());
        //Stats tables
        builder.ApplyConfiguration(new NotificationConfiguration());
        builder.ApplyConfiguration(new NotificationDeliveryConfiguration());
        builder.ApplyConfiguration(new NotificationPreferenceConfiguration());
        // ===== NAMING CONVENTION =====
        builder.UseSnakeCaseNamingConvention();
    }
}
