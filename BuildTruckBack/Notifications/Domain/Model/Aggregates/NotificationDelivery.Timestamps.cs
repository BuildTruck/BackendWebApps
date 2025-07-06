using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckBack.Notifications.Domain.Model.Aggregates;

public partial class NotificationDelivery : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] 
    public DateTimeOffset? CreatedDate { get; set; }
    
    [Column("UpdatedAt")] 
    public DateTimeOffset? UpdatedDate { get; set; }
}