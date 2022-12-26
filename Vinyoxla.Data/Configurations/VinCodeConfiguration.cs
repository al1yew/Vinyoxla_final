using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Core.Models;

namespace Vinyoxla.Data.Configurations
{
    public class VinCodeConfiguration : IEntityTypeConfiguration<VinCode>
    {
        public void Configure(EntityTypeBuilder<VinCode> builder)
        {
            builder.Property(x => x.FileName).IsRequired();
            builder.Property(x => x.Vin).IsRequired().HasMaxLength(17);
        }
    }
}
