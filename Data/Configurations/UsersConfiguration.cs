using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static std.Models.Base;

namespace std.Data.Configurations
{
    public class UsersConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.UserId);

            // 设置添加索引
            builder.HasIndex(u => u.Username);

            // 为Status属性设置默认值为1（如果缺少该值）
            builder.Property(u => u.Status)
                .HasDefaultValue(1);
        }
    }
}