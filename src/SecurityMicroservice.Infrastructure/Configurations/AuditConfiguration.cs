using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable("Audits");
        
        builder.HasKey(a => a.AuditId);
        
        builder.Property(a => a.Entity)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(a => a.Timestamp)
            .IsRequired();
        
        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);
        
        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);
        
        builder.HasOne(a => a.User)
            .WithMany(u => u.Audits)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(a => new { a.Entity, a.Timestamp });
        builder.HasIndex(a => a.UserId);
    }
}