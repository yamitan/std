using Microsoft.EntityFrameworkCore;
using std.Data.Configurations;
using static std.Models.Base;

namespace std.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Class> Classes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 应用实体配置
            modelBuilder.ApplyConfiguration(new UsersConfiguration());
            modelBuilder.ApplyConfiguration(new ClassConfiguration());

            // 应用实体关系配置
            modelBuilder.ApplyConfiguration(new EntityRelationships());
        }
    }
}