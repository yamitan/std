using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static std.Models.Base;

namespace std.Data.Configurations
{
    public class EntityRelationships : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            // 这个配置类专门用于配置实体之间的关系
            // 例如: 用户和班级的关系、年龄组和班级的关系等
            
            // 示例: 如果我们在Users类中添加了ClassId属性，可以配置用户与班级的多对一关系
            // builder.HasOne<Class>()
            //     .WithMany()
            //     .HasForeignKey("ClassId")
            //     .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 