namespace BuildTruckBack.Stats.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

/// <summary>
/// EF Core configuration for StatsHistory aggregate
/// </summary>
public class StatsHistoryConfiguration : IEntityTypeConfiguration<StatsHistory>
{
    public void Configure(EntityTypeBuilder<StatsHistory> builder)
    {
        // Table configuration
        builder.ToTable("StatsHistory");
        builder.HasKey(h => h.Id);

        // Primary properties
        builder.Property(h => h.Id)
            .ValueGeneratedOnAdd();

        builder.Property(h => h.ManagerId)
            .IsRequired();

        builder.Property(h => h.ManagerStatsId)
            .IsRequired();

        // StatsPeriod value object configuration
        builder.OwnsOne(h => h.Period, period =>
        {
            period.WithOwner().HasForeignKey("Id");
            period.Property(p => p.StartDate)
                .HasColumnName("PeriodStartDate")
                .IsRequired();

            period.Property(p => p.EndDate)
                .HasColumnName("PeriodEndDate")
                .IsRequired();

            period.Property(p => p.PeriodType)
                .HasColumnName("PeriodType")
                .HasMaxLength(50)
                .IsRequired();

            period.Property(p => p.DisplayName)
                .HasColumnName("PeriodDisplayName")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Period information
        builder.Property(h => h.SnapshotDate)
            .IsRequired();

        builder.Property(h => h.PeriodType)
            .HasMaxLength(50)
            .IsRequired();

        // Performance snapshot
        builder.Property(h => h.OverallPerformanceScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(h => h.PerformanceGrade)
            .HasMaxLength(2)
            .IsRequired();

        // Project metrics snapshot
        builder.Property(h => h.TotalProjects)
            .IsRequired();

        builder.Property(h => h.ActiveProjects)
            .IsRequired();

        builder.Property(h => h.CompletedProjects)
            .IsRequired();

        builder.Property(h => h.ProjectCompletionRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Personnel metrics snapshot
        builder.Property(h => h.TotalPersonnel)
            .IsRequired();

        builder.Property(h => h.ActivePersonnel)
            .IsRequired();

        builder.Property(h => h.PersonnelActiveRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(h => h.PersonnelEfficiencyScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Incident metrics snapshot
        builder.Property(h => h.TotalIncidents)
            .IsRequired();

        builder.Property(h => h.CriticalIncidents)
            .IsRequired();

        builder.Property(h => h.SafetyScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Material metrics snapshot
        builder.Property(h => h.TotalMaterials)
            .IsRequired();

        builder.Property(h => h.MaterialsOutOfStock)
            .IsRequired();

        builder.Property(h => h.TotalMaterialCost)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(h => h.InventoryHealthScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Machinery metrics snapshot
        builder.Property(h => h.TotalMachinery)
            .IsRequired();

        builder.Property(h => h.ActiveMachinery)
            .IsRequired();

        builder.Property(h => h.MachineryAvailabilityRate)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(h => h.MachineryEfficiencyScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        // Metadata
        builder.Property(h => h.DataSource)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Notes)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(h => h.IsManualSnapshot)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(h => h.ManagerId)
            .HasDatabaseName("IX_StatsHistory_ManagerId");

        builder.HasIndex(h => h.ManagerStatsId)
            .HasDatabaseName("IX_StatsHistory_ManagerStatsId");

        builder.HasIndex(h => h.SnapshotDate)
            .HasDatabaseName("IX_StatsHistory_SnapshotDate");

        builder.HasIndex(h => h.PeriodType)
            .HasDatabaseName("IX_StatsHistory_PeriodType");

        builder.HasIndex(h => h.IsManualSnapshot)
            .HasDatabaseName("IX_StatsHistory_IsManualSnapshot");

        builder.HasIndex(h => h.OverallPerformanceScore)
            .HasDatabaseName("IX_StatsHistory_PerformanceScore");

        // Composite indexes for common queries
        builder.HasIndex(h => new { h.ManagerId, h.SnapshotDate })
            .HasDatabaseName("IX_StatsHistory_Manager_Date");

        builder.HasIndex(h => new { h.ManagerId, h.PeriodType })
            .HasDatabaseName("IX_StatsHistory_Manager_PeriodType");

        builder.HasIndex(h => new { h.ManagerId, h.IsManualSnapshot })
            .HasDatabaseName("IX_StatsHistory_Manager_Manual");

        builder.HasIndex(h => new { h.ManagerId, h.PeriodType, h.SnapshotDate })
            .HasDatabaseName("IX_StatsHistory_Manager_Period_Date");
        
        builder.HasIndex(h => h.ManagerId)
            .HasDatabaseName("IX_StatsHistory_ManagerId");
        
        builder.HasIndex(h => h.ManagerStatsId)
            .HasDatabaseName("IX_StatsHistory_ManagerStatsId");
    
        // Para el diagrama:
        builder.Property(h => h.ManagerId)
            .HasComment("References Users.Id");
        
        builder.Property(h => h.ManagerStatsId)
            .HasComment("References ManagerStats.Id");
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(h => h.ManagerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_StatsHistory_Users_ManagerId");
    
        // FK to ManagerStats table
        builder.HasOne<ManagerStats>()
            .WithMany()
            .HasForeignKey(h => h.ManagerStatsId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StatsHistory_ManagerStats_ManagerStatsId");
        // Foreign key constraint (optional, since we're using DDD approach)
        // builder.HasOne<ManagerStats>()
        //     .WithMany()
        //     .HasForeignKey(h => h.ManagerStatsId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // Audit fields (CreatedDate, UpdatedDate handled by IEntityWithCreatedUpdatedDate)
    }
}