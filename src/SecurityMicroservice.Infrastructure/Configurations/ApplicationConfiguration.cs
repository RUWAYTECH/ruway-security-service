using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");
        
        builder.HasKey(a => a.ApplicationId);
        
        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.BaseUrl)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(a => a.IsActive)
            .IsRequired();
        
        builder.Property(a => a.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(a => a.Code)
            .IsUnique();
    }
}