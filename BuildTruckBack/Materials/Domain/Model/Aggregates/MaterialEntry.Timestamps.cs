using System;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckBack.Materials.Domain.Model.Aggregates
{
    public partial class MaterialEntry : IEntityWithCreatedUpdatedDate
    {
        [Column("CreatedAt")]
        public DateTimeOffset? CreatedDate { get; set; }

        [Column("UpdatedAt")]
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}