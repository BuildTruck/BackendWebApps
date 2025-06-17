using BuildTruckBack.Projects.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Machinery.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Domain.Model.Aggregates.Machinery> Machinery { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User Configuration
        builder.Entity<User>().ToTable("users");
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(100);
        //builder.Entity<User>().Property(u => u.Password).IsRequired();
        builder.Entity<User>().Property(u => u.Role).IsRequired();

        // Project Configuration
        builder.Entity<Project>().ToTable("projects");
        builder.Entity<Project>().HasKey(p => p.Id);
        builder.Entity<Project>().Property(p => p.Id).IsRequired().HasMaxLength(50);
        builder.Entity<Project>().Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Entity<Project>().Property(p => p.Location).IsRequired().HasMaxLength(100);
        builder.Entity<Project>().Property(p => p.State).IsRequired().HasMaxLength(50);
        builder.Entity<Project>().Property(p => p.Description).HasMaxLength(500);
        builder.Entity<Project>().Property(p => p.StartDate).IsRequired();
        builder.Entity<Project>().Property(p => p.SupervisorId).HasMaxLength(50);

        // Machinery Configuration
        builder.Entity<Domain.Model.Aggregates.Machinery>().ToTable("machinery");
        builder.Entity<Domain.Model.Aggregates.Machinery>().HasKey(m => m.Id);
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.LicensePlate).IsRequired().HasMaxLength(20);
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.RegisterDate).IsRequired();
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.Status).IsRequired().HasConversion<string>();
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.Provider).IsRequired().HasMaxLength(100);
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.Description).HasMaxLength(500);
        builder.Entity<Domain.Model.Aggregates.Machinery>().Property(m => m.ProjectId).IsRequired().HasMaxLength(50);

        // Machinery-Project Relationship
        builder.Entity<Domain.Model.Aggregates.Machinery>()
            .HasOne(m => m.Project)
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .HasConstraintName("FK_Machinery_Projects_ProjectId");

        // Ensure LicensePlate is unique
        builder.Entity<Domain.Model.Aggregates.Machinery>()
            .HasIndex(m => m.LicensePlate)
            .IsUnique();
    }
}