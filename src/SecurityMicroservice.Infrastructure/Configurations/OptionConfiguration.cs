using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable("Options");
        
        builder.HasKey(o => o.OptionId);
        
        builder.Property(o => o.Module)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(o => o.Route)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(o => o.HttpMethod)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(o => o.IsActive)
            .IsRequired();
        
        builder.Property(o => o.CreatedAt)
            .IsRequired();
        
        builder.HasOne(o => o.Application)
            .WithMany(a => a.Options)
            .HasForeignKey(o => o.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(o => new { o.ApplicationId, o.Route, o.HttpMethod })
            .IsUnique();
    }
}