using CassMach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CassMach.Infrastructure.Persistence.EntityConfigurations
{
    public class ErrorSolutionConfiguration : IEntityTypeConfiguration<ErrorSolution>
    {
        public void Configure(EntityTypeBuilder<ErrorSolution> builder)
        {
            builder.ToTable("ErrorSolutions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConversationId)
                .IsRequired();

            builder.HasIndex(x => x.ConversationId);

            builder.Property(x => x.UserQuestion)
                .IsRequired();

            builder.Property(x => x.Brand)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Model)
                .HasMaxLength(100);

            builder.Property(x => x.ErrorCode)
                .HasMaxLength(50);

            builder.Property(x => x.AiResponse)
                .IsRequired();

            builder.Property(x => x.AttemptNumber)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.FromCache)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.InputTokens)
                .HasDefaultValue(0);

            builder.Property(x => x.OutputTokens)
                .HasDefaultValue(0);

            builder.Property(x => x.CreditsCharged)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            builder.HasOne(x => x.User)
                .WithMany(x => x.ErrorSolutions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.Brand, x.ErrorCode });
        }
    }
}
