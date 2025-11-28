using System.Diagnostics;
using System.Text.Json;
using DailyPlant.Library.Models;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public class PlantApiService : IPlantApiService
{
    private const string API_KEY = "LD2JJbOND5SooohHCN44kezi";
    private const string SECRET_KEY = "F7xYr8OLo2jAfTu1wvjj0Eiew7tndZ3u";
    private readonly HttpClient _httpClient;

    public PlantApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", API_KEY),
                new("client_secret", SECRET_KEY)
            };

            var response = await _httpClient.PostAsync(
                "https://aip.baidubce.com/oauth/2.0/token",
                new FormUrlEncodedContent(parameters));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                {
                    return tokenElement.GetString();
                }
            }

            Debug.WriteLine($"获取访问令牌失败: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取访问令牌异常: {ex.Message}");
            throw new Exception($"获取访问令牌时出错: {ex.Message}");
        }
    }

    public async Task<PlantRecognitionResult> RecognizePlantAsync(string base64Image)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("无法获取有效的访问令牌");
            }

            var parameters = new List<KeyValuePair<string, string>>
            {
                new("image", base64Image),
                new("baike_num", "1")
            };

            var url = $"https://aip.baidubce.com/rest/2.0/image-classify/v1/plant?access_token={accessToken}";
            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(parameters));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PlantRecognitionResult>(
                    json, 
                    new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true 
                    });
                
                Debug.WriteLine($"API识别成功，找到 {result?.Result?.Count ?? 0} 个植物");
                return result;
            }
            else
            {
                throw new Exception($"API请求失败: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"植物识别API调用异常: {ex.Message}");
            throw new Exception($"植物识别失败: {ex.Message}");
        }
    }

    public async Task<HttpResponseMessage> SendApiRequestAsync(string url, HttpContent content)
    {
        return await _httpClient.PostAsync(url, content);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}