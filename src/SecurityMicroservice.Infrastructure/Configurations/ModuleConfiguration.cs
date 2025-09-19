using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules");
        
        builder.HasKey(m => m.ModuleId);
        
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(m => m.Icon)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(m => m.Order)
            .IsRequired();
        
        builder.HasOne(m => m.Application)
            .WithMany(a => a.Modules)
            .HasForeignKey(m => m.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    
    }
}