using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static std.Models.Base;

namespace std.Data.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.ToTable("Classes");
            builder.HasKey(c => c.ClassId);
            
            // 可以在这里添加Class表特定的配置
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);
        }
    }
} 