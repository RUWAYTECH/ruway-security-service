using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class UserApplicationConfiguration : IEntityTypeConfiguration<UserApplication>
{
    public void Configure(EntityTypeBuilder<UserApplication> builder)
    {
        builder.ToTable("UserApplications");
        
        builder.HasKey(ua => new { ua.UserId, ua.ApplicationId });
        
        builder.Property(ua => ua.IsActive)
            .IsRequired();
        
        builder.Property(ua => ua.AssignedAt)
            .IsRequired();
        
        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserApplications)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ua => ua.Application)
            .WithMany(a => a.UserApplications)
            .HasForeignKey(ua => ua.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}