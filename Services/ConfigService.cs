using std.Models;

namespace std.Services
{
    /// <summary>
    /// 配置服务，用于读取 appsettings.json 中的配置
    /// </summary>
    public class ConfigService
    {
        private readonly IConfiguration _configuration;

        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <returns>配置值</returns>
        public string GetValue(string key)
        {
            return _configuration[key];
        }

        /// <summary>
        /// 获取配置节
        /// </summary>
        /// <param name="section">配置节名称</param>
        /// <returns>配置节</returns>
        public IConfigurationSection GetSection(string section)
        {
            return _configuration.GetSection(section);
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns>数据库连接字符串</returns>
        public string GetDatabasePath()
        {
            return _configuration["Database:ConnectionString"];
        }

        public Jwt GetJwtSetting() { 
            return GetSection("Jwt").Get<Jwt>();
        }
    }
} 