using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckBack.Notifications.Infrastructure.Persistence.EFC.Configuration;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        
        builder.HasKey(np => np.Id);
        builder.Property(np => np.Id).ValueGeneratedOnAdd();
        
        builder.Property(np => np.UserId)
            .IsRequired();
            
        builder.Property(np => np.Context)
            .HasConversion(
                c => c.Value,
                c => BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationContext.FromString(c))
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(np => np.InAppEnabled)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(np => np.EmailEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(np => np.MinimumPriority)
            .HasConversion(
                p => p.Value,
                p => BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationPriority.FromString(p))
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(np => np.CreatedDate)
            .HasColumnName("CreatedAt")
            .IsRequired();
            
        builder.Property(np => np.UpdatedDate)
            .HasColumnName("UpdatedAt");
            
        builder.HasIndex(np => np.UserId);
        builder.HasIndex(np => new { np.UserId, np.Context })
            .IsUnique();
        builder.HasOne<BuildTruckBack.Users.Domain.Model.Aggregates.User>()
            .WithMany()
            .HasForeignKey(np => np.UserId)  // ✅ Esta propiedad SÍ existe
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_NotificationPreferences_Users_UserId");
    }
    
}