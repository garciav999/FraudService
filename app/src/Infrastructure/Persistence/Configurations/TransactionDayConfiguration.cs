using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TransactionDayConfiguration : IEntityTypeConfiguration<TransactionDay>
{
    public void Configure(EntityTypeBuilder<TransactionDay> builder)
    {
        builder.ToTable("TransactionDay");

        builder.HasKey(td => td.TransactionDayId);

        builder.Property(td => td.TransactionDayId)
            .HasColumnName("TransactionDayId")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(td => td.TransactionDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(td => td.SourceAccountId)
            .IsRequired();

        builder.Property(td => td.TotalValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(td => td.UpdateAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
