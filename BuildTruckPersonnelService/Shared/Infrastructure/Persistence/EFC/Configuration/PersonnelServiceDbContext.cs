using BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;
using BuildTruckPersonnelService.Personnel.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class PersonnelServiceDbContext(DbContextOptions<PersonnelServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<Personnel> Personnel { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new PersonnelConfiguration());
        builder.UseSnakeCaseNamingConvention();
    }
}
