namespace std.Models
{
    public class Jwt
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 服务名称
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 有效期
        /// </summary>
        public int ExpireMinutes { get; set; }
    }
}
