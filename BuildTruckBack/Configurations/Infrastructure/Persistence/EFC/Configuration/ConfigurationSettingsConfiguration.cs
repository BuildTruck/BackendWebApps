using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckBack.Configurations.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Entity Framework configuration for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsConfiguration : IEntityTypeConfiguration<ConfigurationSettings>
{
    public void Configure(EntityTypeBuilder<ConfigurationSettings> builder)
    {
        builder.ToTable("configurations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(c => c.Themes).HasColumnName("theme").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Plans).HasColumnName("plan").HasMaxLength(20).IsRequired();
        builder.Property(c => c.NotificationsEnable).HasColumnName("notifications_enable").IsRequired();
        builder.Property(c => c.EmailNotifications).HasColumnName("email_notifications").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();
    
        // ← CAMBIAR esto:
        builder.Property(c => c.TutorialsCompleted)
            .HasColumnName("tutorials_completed")
            .HasMaxLength(1000)
            .IsRequired()
            .HasConversion(
                v => v.ToJsonString(),
                v => new TutorialProgress(v)
            );

        builder.HasIndex(c => c.UserId).IsUnique().HasDatabaseName("ix_configurations_user_id");
        // FK Configurations->Users exists in MySQL but not registered in EF (owned by UserServiceDbContext)
    }
}