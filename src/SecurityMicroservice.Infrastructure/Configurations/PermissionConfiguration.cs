using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        
        builder.HasKey(p => p.PermissionId);
        
        builder.Property(p => p.ActionCode)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.IsActive)
            .IsRequired();
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.HasOne(p => p.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(p => p.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(p => p.Option)
            .WithMany(o => o.Permissions)
            .HasForeignKey(p => p.OptionId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasIndex(p => new { p.RoleId, p.OptionId, p.ActionCode })
            .IsUnique();
    }
}