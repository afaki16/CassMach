using CassMach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CassMach.Infrastructure.Persistence.EntityConfigurations
{
    public class TokenTransactionConfiguration : IEntityTypeConfiguration<TokenTransaction>
    {
        public void Configure(EntityTypeBuilder<TokenTransaction> builder)
        {
            builder.ToTable("TokenTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.CreditAmount)
                .HasColumnType("decimal(18,4)");

            builder.Property(x => x.Multiplier)
                .HasColumnType("decimal(18,4)");

            builder.Property(x => x.BalanceAfter)
                .HasColumnType("decimal(18,4)");

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => x.ReferenceId);

            builder.HasOne(x => x.User)
                .WithMany(x => x.TokenTransactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
