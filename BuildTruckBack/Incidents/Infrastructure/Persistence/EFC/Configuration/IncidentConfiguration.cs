using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.ValueObjects;
using BuildTruckBack.Projects.Domain.Model.Aggregates; // ✅ Agregar este using
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckBack.Incidents.Infrastructure.Persistence.EFC.Configuration;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        // Table configuration
        builder.ToTable("Incidents");
        builder.HasKey(i => i.Id);

        // Primary properties
        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.ProjectId)
            .IsRequired(false)
            .HasComment("References Projects.Id");

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.IncidentType)
            .IsRequired()
            .HasMaxLength(100);

        // Enum configurations
        builder.Property(i => i.Severity)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.ReportedBy)
            .HasMaxLength(100);

        builder.Property(i => i.AssignedTo)
            .HasMaxLength(100);

        builder.Property(i => i.OccurredAt)
            .IsRequired();

        builder.Property(i => i.ResolvedAt)
            .IsRequired(false);

        builder.Property(i => i.Image)
            .HasMaxLength(1000); // URL de Cloudinary

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        builder.Property(i => i.RegisterDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(i => i.ProjectId)
            .HasDatabaseName("IX_Incidents_ProjectId");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_Incidents_Status");

        builder.HasIndex(i => i.Severity)
            .HasDatabaseName("IX_Incidents_Severity");

        builder.HasIndex(i => i.OccurredAt)
            .HasDatabaseName("IX_Incidents_OccurredAt");

        // ✅ Foreign Key a Projects
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.SetNull) // Si se elimina el proyecto, los incidents quedan huérfanos
            .HasConstraintName("FK_Incidents_Projects_ProjectId");
    }
}