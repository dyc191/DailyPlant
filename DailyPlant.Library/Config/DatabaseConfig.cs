namespace DailyPlant.Library.Config
{
    public static class DatabaseConfig
    {
        public static string GetDatabasePath()
        {
            // 尝试多种可能的路径
            var possiblePaths = new[]
            {
                // 开发环境：在解决方案目录中
                Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\DailyPlant.Library\plantdb.sqlite3"),
                // 发布环境：在应用程序目录中
                Path.Combine(AppContext.BaseDirectory, "plantdb.sqlite3"),
                // 备用路径
                Path.Combine(Directory.GetCurrentDirectory(), "plantdb.sqlite3"),
                // 类库项目中的路径
                Path.Combine(AppContext.BaseDirectory, "..", "DailyPlant.Library", "plantdb.sqlite3")
            };
            
            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                System.Diagnostics.Debug.WriteLine($"检查数据库路径: {fullPath}");
                
                if (File.Exists(fullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"找到数据库文件: {fullPath}");
                    return fullPath;
                }
            }
            
            // 如果没有找到，返回第一个路径（将在此位置创建新数据库）
            var defaultPath = Path.GetFullPath(possiblePaths[0]);
            System.Diagnostics.Debug.WriteLine($"使用默认数据库路径: {defaultPath}");
            return defaultPath;
        }
    }
}