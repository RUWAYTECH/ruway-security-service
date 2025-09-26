using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        
        builder.HasKey(r => r.RoleId);
        
        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(r => r.Description)
            .HasMaxLength(500);
        
        //builder.Property(r => r.Url)
        //    .HasMaxLength(500);
        
        builder.Property(r => r.IsActive)
            .IsRequired();
        
        builder.Property(r => r.CreatedAt)
            .IsRequired();
        
        builder.HasOne(r => r.Application)
            .WithMany(a => a.Roles)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(r => new { r.ApplicationId, r.Code })
            .IsUnique();
    }
}