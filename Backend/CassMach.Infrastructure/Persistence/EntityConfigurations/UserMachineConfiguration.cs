using CassMach.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CassMach.Infrastructure.Persistence.EntityConfigurations
{
    public class UserMachineConfiguration : IEntityTypeConfiguration<UserMachine>
    {
        public void Configure(EntityTypeBuilder<UserMachine> builder)
        {
            builder.ToTable("UserMachines");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(150);

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserMachines)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Machine)
                .WithMany(x => x.UserMachines)
                .HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bir kullanıcı aynı makineyi bir kez ekleyebilir
            builder.HasIndex(x => new { x.UserId, x.MachineId }).IsUnique();
            builder.HasIndex(x => x.UserId);
        }
    }
}
