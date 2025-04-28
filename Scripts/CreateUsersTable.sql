-- 创建用户表
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    Phone TEXT,
    RealName TEXT,
    Role TEXT NOT NULL DEFAULT 'user',
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL,
    LastLoginAt DATETIME
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_role ON Users(Role);

-- 插入管理员用户 (密码: admin123)
-- 密码哈希是使用PBKDF2-SHA256算法生成的
INSERT OR IGNORE INTO Users (
    Username, 
    PasswordHash, 
    Email, 
    RealName, 
    Role, 
    IsActive, 
    CreatedAt
) VALUES (
    'admin',
    'VGhpcyBpcyBhIGR1bW15IGhhc2ggLSByZXBsYWNlIHdpdGggcmVhbCBwYXNzd29yZCBoYXNo',  -- 这只是一个示例哈希，实际中应该使用真实哈希
    'admin@example.com',
    '系统管理员',
    'admin',
    1,
    datetime('now')
); 