using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace DailyPlant.Library.Services;

public interface IImageTextCombinerService
{
    Task<string> CreateCombinedImageAsync(string textContent, Bitmap originalImage);
}