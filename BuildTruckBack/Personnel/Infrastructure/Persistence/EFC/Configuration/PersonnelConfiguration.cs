using BuildTruckBack.Personnel.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckBack.Personnel.Infrastructure.Persistence.EFC.Configuration;

public class PersonnelConfiguration : IEntityTypeConfiguration<Domain.Model.Aggregates.Personnel>
{
    public void Configure(EntityTypeBuilder<Domain.Model.Aggregates.Personnel> builder)
    {
        // Table configuration
        builder.ToTable("Personnel");
        builder.HasKey(p => p.Id);

        // Primary key
        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        // Basic Information
        builder.Property(p => p.ProjectId)
            .HasColumnName("ProjectId")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Lastname)
            .HasColumnName("Lastname")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DocumentNumber)
            .HasColumnName("DocumentNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Position)
            .HasColumnName("Position")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Department)
            .HasColumnName("Department")
            .HasMaxLength(100)
            .IsRequired();

        // Enum configurations
        builder.Property(p => p.PersonnelType)
            .HasColumnName("PersonnelType")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        // Financial Information
        builder.Property(p => p.MonthlyAmount)
            .HasColumnName("MonthlyAmount")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(p => p.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(p => p.Discount)
            .HasColumnName("Discount")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(p => p.Bank)
            .HasColumnName("Bank")
            .HasMaxLength(50);

        builder.Property(p => p.AccountNumber)
            .HasColumnName("AccountNumber")
            .HasMaxLength(50);

        // Contract Information
        builder.Property(p => p.StartDate)
            .HasColumnName("StartDate")
            .HasColumnType("date");

        builder.Property(p => p.EndDate)
            .HasColumnName("EndDate")
            .HasColumnType("date");

        // Monthly Attendance Calculated Fields
        builder.Property(p => p.WorkedDays)
            .HasColumnName("WorkedDays")
            .HasDefaultValue(0);

        builder.Property(p => p.CompensatoryDays)
            .HasColumnName("CompensatoryDays")
            .HasDefaultValue(0);

        builder.Property(p => p.UnpaidLeave)
            .HasColumnName("UnpaidLeave")
            .HasDefaultValue(0);

        builder.Property(p => p.Absences)
            .HasColumnName("Absences")
            .HasDefaultValue(0);

        builder.Property(p => p.Sundays)
            .HasColumnName("Sundays")
            .HasDefaultValue(0);

        builder.Property(p => p.TotalDays)
            .HasColumnName("TotalDays")
            .HasDefaultValue(0);

        // Monthly Attendance JSON
        builder.Property(p => p.MonthlyAttendanceJson)
            .HasColumnName("MonthlyAttendanceJson")
            .HasColumnType("json")
            .HasDefaultValue("{}");

        // Contact Information
        builder.Property(p => p.Phone)
            .HasColumnName("Phone")
            .HasMaxLength(20);

        builder.Property(p => p.Email)
            .HasColumnName("Email")
            .HasMaxLength(150);

        builder.Property(p => p.AvatarUrl)
            .HasColumnName("AvatarUrl")
            .HasColumnType("text");

        // Audit Fields
        builder.Property(p => p.IsDeleted)
            .HasColumnName("IsDeleted")
            .HasDefaultValue(false);

        // Timestamps (handled by EntityFrameworkCore.CreatedUpdatedDate)
        builder.Property(p => p.CreatedDate)
            .HasColumnName("CreatedAt");

        builder.Property(p => p.UpdatedDate)
            .HasColumnName("UpdatedAt");

        // Indexes for performance
        builder.HasIndex(p => p.ProjectId)
            .HasDatabaseName("IX_Personnel_ProjectId");

        builder.HasIndex(p => new { p.ProjectId, p.DocumentNumber })
            .HasDatabaseName("IX_Personnel_ProjectId_DocumentNumber")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0"); // Unique only for non-deleted records

        builder.HasIndex(p => new { p.ProjectId, p.Email })
            .HasDatabaseName("IX_Personnel_ProjectId_Email")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [Email] IS NOT NULL AND [Email] != ''");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Personnel_Status");

        builder.HasIndex(p => p.PersonnelType)
            .HasDatabaseName("IX_Personnel_PersonnelType");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Personnel_IsDeleted");

        // Ignore the computed property
        builder.Ignore(p => p.MonthlyAttendance);
    }
}