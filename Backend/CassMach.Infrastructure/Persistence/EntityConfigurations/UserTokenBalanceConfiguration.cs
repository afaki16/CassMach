using CassMach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CassMach.Infrastructure.Persistence.EntityConfigurations
{
    public class UserTokenBalanceConfiguration : IEntityTypeConfiguration<UserTokenBalance>
    {
        public void Configure(EntityTypeBuilder<UserTokenBalance> builder)
        {
            builder.ToTable("UserTokenBalances");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.Balance)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            builder.Property(x => x.TotalRawTokensUsed)
                .HasDefaultValue(0L);

            builder.Property(x => x.TotalCreditsUsed)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            builder.HasOne(x => x.User)
                .WithOne(x => x.TokenBalance)
                .HasForeignKey<UserTokenBalance>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
