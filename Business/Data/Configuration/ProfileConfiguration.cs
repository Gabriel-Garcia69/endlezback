using Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Business.Data.Configuration
{
    public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(pi => pi.Id)
                .HasDefaultValueSql("newid()");

            builder.Property(p => p.Title)
                .HasColumnType("varchar")
                .IsRequired()
                .HasMaxLength(125);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512)
                .HasDefaultValue("");
        }
    }
}
