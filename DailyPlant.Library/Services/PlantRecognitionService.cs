using System.Diagnostics;
using Avalonia.Media.Imaging;
using DailyPlant.Library.Models;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public class PlantRecognitionService : IPlantRecognitionService
{
    private readonly IPlantApiService _plantApiService;
    private readonly HttpClient _httpClient;

    public PlantRecognitionService(IPlantApiService plantApiService)
    {
        _plantApiService = plantApiService ?? throw new ArgumentNullException(nameof(plantApiService));
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<PlantRecognitionResult> RecognizePlantAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            throw new ArgumentException("图片路径不能为空");

        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"图片文件不存在: {imagePath}");

        // 验证图片格式
        if (!await ValidateImageAsync(imagePath))
            throw new ArgumentException("无效的图片文件");

        try
        {
            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string base64Image = Convert.ToBase64String(imageBytes);

            Debug.WriteLine($"开始识别植物，图片大小: {imageBytes.Length} 字节");
            return await _plantApiService.RecognizePlantAsync(base64Image);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"植物识别服务异常: {ex.Message}");
            throw new Exception($"植物识别失败: {ex.Message}", ex);
        }
    }

    public async Task<PlantRecognitionResult> RecognizePlantFromBytesAsync(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("图片数据不能为空");

        try
        {
            string base64Image = Convert.ToBase64String(imageBytes);
            Debug.WriteLine($"从字节数组识别植物，数据大小: {imageBytes.Length} 字节");
            return await _plantApiService.RecognizePlantAsync(base64Image);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"从字节数组识别植物异常: {ex.Message}");
            throw new Exception($"植物识别失败: {ex.Message}", ex);
        }
    }

    public async Task<string> CapturePhotoAsync()
    {
        try
        {
            // 创建临时目录
            var tempDir = Path.Combine(Path.GetTempPath(), "PlantRecognition");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            // 生成唯一的文件名
            var fileName = $"plant_photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var targetPath = Path.Combine(tempDir, fileName);

            // 使用 Windows 相机应用
            if (OperatingSystem.IsWindows())
            {
                return await CaptureWithWindowsCameraApp(tempDir, targetPath);
            }

            throw new PlatformNotSupportedException("当前平台不支持拍照功能");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"拍照服务异常: {ex.Message}");
            throw new Exception($"拍照失败: {ex.Message}", ex);
        }
    }

    public async Task<Bitmap> LoadPlantImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.WriteLine("图片URL为空，跳过加载");
            return null;
        }

        try
        {
            Debug.WriteLine($"开始加载植物图片: {imageUrl}");
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            Debug.WriteLine($"图片下载完成，大小: {imageBytes.Length} 字节");

            using var memoryStream = new MemoryStream(imageBytes);
            var bitmap = new Bitmap(memoryStream);
            Debug.WriteLine("Bitmap创建成功");
            return bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"加载植物图片失败: {ex.Message}");
            return null;
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            return await _plantApiService.GetAccessTokenAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取访问令牌服务异常: {ex.Message}");
            throw new Exception($"获取访问令牌失败: {ex.Message}", ex);
        }
    }

    public async Task<bool> ValidateImageAsync(string imagePath)
    {
        try
        {
            if (!File.Exists(imagePath))
                return false;

            var fileInfo = new FileInfo(imagePath);
            if (fileInfo.Length == 0)
                return false;

            // 检查文件扩展名
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            var extension = Path.GetExtension(imagePath).ToLower();
            if (!validExtensions.Contains(extension))
                return false;

            // 尝试读取图片文件头验证格式
            byte[] header = new byte[8];
            using (var stream = File.OpenRead(imagePath))
            {
                await stream.ReadAsync(header, 0, header.Length);
            }

            // 简单的图片文件头验证
            return IsValidImageHeader(header);
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidImageHeader(byte[] header)
    {
        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            return true;

        // BMP: 42 4D
        if (header[0] == 0x42 && header[1] == 0x4D)
            return true;

        return false;
    }

    private async Task<string> CaptureWithWindowsCameraApp(string outputDirectory, string targetPath)
    {
        
        var picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        var cameraRollDir = Path.Combine(picturesDir, "Camera Roll");
        var watchDir = Directory.Exists(cameraRollDir) ? cameraRollDir : picturesDir;
        
        var existingFiles = GetImageFiles(watchDir);
        var existingFileSet = new HashSet<string>(existingFiles);

        // 启动相机应用
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "microsoft.windows.camera:",
                UseShellExecute = true
            }
        };
        process.Start();

        // 等待新照片
        var newPhoto = await WaitForNewPhotoFile(watchDir, existingFileSet, TimeSpan.FromSeconds(5));
        
        if (!string.IsNullOrEmpty(newPhoto))
        {
            await WaitForFileReady(newPhoto);
            File.Copy(newPhoto, targetPath, true);
            await CloseCameraApp();
            return targetPath;
        }

        await CloseCameraApp();
        return null;
    }

    private string[] GetImageFiles(string directory)
    {
        try
        {
            return Directory.GetFiles(directory, "*.*")
                .Where(f => f.ToLower().EndsWith(".jpg") || 
                           f.ToLower().EndsWith(".jpeg") || 
                           f.ToLower().EndsWith(".png"))
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private async Task<string> WaitForNewPhotoFile(string directory, HashSet<string> existingFiles, TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        
        while (DateTime.Now - startTime < timeout)
        {
            var currentFiles = GetImageFiles(directory);
            foreach (var file in currentFiles)
            {
                if (!existingFiles.Contains(file) && IsLikelyCameraFile(file))
                {
                    await WaitForFileReady(file);
                    return file;
                }
            }
            await Task.Delay(1000);
        }
        return null;
    }

    private bool IsLikelyCameraFile(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
        var cameraPatterns = new[] { "photo", "img", "win_", "camera", "capture", "picture", "wp_", "dsc" };
        return cameraPatterns.Any(pattern => fileName.Contains(pattern)) ||
               fileName.StartsWith("wp_") ||
               fileName.StartsWith("dsc") ||
               fileName.StartsWith("img_");
    }

    private async Task WaitForFileReady(string filePath)
    {
        const int maxRetries = 20;
        const int delayMs = 500;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (stream.Length > 1024)
                        return;
                }
            }
            catch (IOException)
            {
                // 文件被占用，等待
            }
            await Task.Delay(delayMs);
        }
        throw new TimeoutException($"文件未就绪: {filePath}");
    }

    private async Task CloseCameraApp()
    {
        try
        {
            var processes = Process.GetProcessesByName("WindowsCamera");
            foreach (var process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        await Task.Delay(1000);
                        if (!process.HasExited)
                            process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"关闭相机进程失败: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"关闭相机应用时出错: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}