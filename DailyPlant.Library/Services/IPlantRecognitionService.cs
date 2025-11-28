using Avalonia.Media.Imaging;
using DailyPlant.Library.Models;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public interface IPlantRecognitionService
{
    Task<PlantRecognitionResult> RecognizePlantAsync(string imagePath);
    Task<string> CapturePhotoAsync();
    Task<Bitmap> LoadPlantImageAsync(string imageUrl);
    Task<string> GetAccessTokenAsync();
    Task<bool> ValidateImageAsync(string imagePath);
    Task<PlantRecognitionResult> RecognizePlantFromBytesAsync(byte[] imageBytes);
}