using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.UserId);
        
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.IsExternal)
            .IsRequired();
        
        builder.Property(u => u.CreatedAt)
            .IsRequired();
        

           builder.Property(u => u.CreatedAt);
        

        builder.HasIndex(u => u.UserName)
            .IsUnique();
        
        builder.HasIndex(u => u.EmployeeId);
    }
}