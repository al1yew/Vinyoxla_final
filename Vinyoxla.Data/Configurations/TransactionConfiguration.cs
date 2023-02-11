using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Core.Models;

namespace Vinyoxla.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(x => x.SessionId).IsRequired();
            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.Amount).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
