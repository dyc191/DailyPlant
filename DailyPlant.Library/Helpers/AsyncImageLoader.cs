using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DailyPlant.Library.Helpers
{
    public static class AsyncImageLoader
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, Bitmap> _cache = new Dictionary<string, Bitmap>();

        public static async Task<Bitmap?> LoadImageAsync(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            // 检查缓存
            if (_cache.TryGetValue(url, out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                Bitmap? bitmap = null;

                // 处理本地文件
                if (url.StartsWith("file://") || !url.Contains("://"))
                {
                    var localPath = url.StartsWith("file://") ? url.Substring(7) : url;
                    if (File.Exists(localPath))
                    {
                        // 使用 Task.Run 避免阻塞 UI 线程
                        bitmap = await Task.Run(() => new Bitmap(localPath));
                    }
                }
                // 处理网络图片
                else if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsByteArrayAsync();
                        using var stream = new MemoryStream(data);
                        bitmap = new Bitmap(stream);
                    }
                }

                if (bitmap != null)
                {
                    _cache[url] = bitmap;
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"图片加载失败: {url}, 错误: {ex.Message}");
                return null;
            }
        }
    }
}