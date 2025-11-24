using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DailyPlant.Helpers
{
    public static class ImageLoader
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        public static async Task<Bitmap?> LoadImageAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    System.Diagnostics.Debug.WriteLine($"无效的图片URL: {url}");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"开始加载图片: {url}");
                
                // 如果是本地文件路径
                if (url.StartsWith("file://") || !url.Contains("://"))
                {
                    var localPath = url.Replace("file://", "");
                    if (File.Exists(localPath))
                    {
                        return new Bitmap(localPath);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"本地图片文件不存在: {localPath}");
                        return null;
                    }
                }
                
                // 在线图片
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    using var stream = new MemoryStream(data);
                    return new Bitmap(stream);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"图片加载失败: {url}, 状态码: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"图片加载异常: {ex.Message}");
                return null;
            }
        }
    }
}
