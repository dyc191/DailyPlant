using DailyPlant.Library.Models;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public interface IPlantApiService
{
    Task<string> GetAccessTokenAsync();
    Task<PlantRecognitionResult> RecognizePlantAsync(string base64Image);
    Task<HttpResponseMessage> SendApiRequestAsync(string url, HttpContent content);
}