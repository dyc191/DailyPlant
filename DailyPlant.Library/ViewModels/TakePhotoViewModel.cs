using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels;

public partial class TakePhotoViewModel : ViewModelBase
{
    
    [ObservableProperty]
    private string _selectedImagePath = string.Empty;

    [ObservableProperty]
    private string _recognitionResult = "请选择图片进行植物识别";

    [ObservableProperty]
    private bool _isProcessing = false;
    
    [ObservableProperty]
    private PlantRecognitionResult _plantResult;
    
    
    //Todo
    [ObservableProperty]
    private bool _isTakingPhoto = false;

    [ObservableProperty]
    private string _photoStatus = "准备拍照";
    
    
    private readonly IContentNavigationService _contentNavigationService;

    public TakePhotoViewModel(IContentNavigationService contentNavigationService)
    {
        _contentNavigationService = contentNavigationService;
    }
    

    [RelayCommand]
    private async Task SelectImageAsync()
    {
        try
        {
            // 获取当前窗口
            var window = GetCurrentWindow();
            if (window == null) return;
            
            //获取TopLevel，因为文件选择器需要TopLevel
            var topLevel = TopLevel.GetTopLevel(window);
            if (topLevel == null) return;
            
            //定义一个文件类型过滤器,只允许选择图片文件
            var fileType = new FilePickerFileType("图像文件")
            {
                Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" },
                AppleUniformTypeIdentifiers = new[] { "public.image" },
                MimeTypes = new[] { "image/*" }
            };
            
            var options = new FilePickerOpenOptions
            {
                Title = "选择植物图片",
                FileTypeFilter = new[] { fileType },
                AllowMultiple = false
            };
            
            
            //调用文件选择器，让用户选择一张图片
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            
            if (files.Count > 0 && files[0] is IStorageFile file)
            {
                SelectedImagePath = file.Path.LocalPath;
                //如果用户选择了文件，将文件的本地路径赋值给SelectedImagePath
                RecognitionResult = "图片已选择，点击识别按钮进行识别";
            }
        }
        catch (Exception ex)
        {
            RecognitionResult = $"选择图片失败: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task RecognizePlantAsync()
    {
        if (string.IsNullOrEmpty(SelectedImagePath) || !File.Exists(SelectedImagePath))
        {
            RecognitionResult = "请先选择图片";
            return;
        }
    
        try
        {
            IsProcessing = true;
            RecognitionResult = "识别中...";
        
            byte[] imageBytes = await File.ReadAllBytesAsync(SelectedImagePath);
            string base64Image = Convert.ToBase64String(imageBytes);
        
            string accessToken = await GetAccessTokenAsync();
        
            if (string.IsNullOrEmpty(accessToken))
            {
                RecognitionResult = "获取访问令牌失败，请检查API密钥配置";
                return;
            }
        
            // 调用植物识别API
            var result = await RecognizePlantApiAsync(base64Image, accessToken);
        
            if (result?.Result?.Count > 0)
            {
                var plant = result.Result[0];
                Debug.WriteLine($"识别到植物: {plant.Name}, 分数: {plant.Score}");
                Debug.WriteLine($"百科信息: {plant.BaikeInfo != null}");
                if (plant.BaikeInfo != null)
                {
                    Debug.WriteLine($"图片URL: {plant.BaikeInfo.ImageUrl}");
                    Debug.WriteLine($"描述长度: {plant.BaikeInfo.Description?.Length ?? 0}");
                }
            
                // 跳转到详情页面
              _contentNavigationService.NavigateTo(ContentNavigationConstant.PlantDetailView, result);
            }
            else
            {
                RecognitionResult = "识别失败，未找到匹配的植物";
            }
        }
        catch (Exception ex)
        {
            RecognitionResult = $"识别失败: {ex.Message}";
            Debug.WriteLine($"识别异常: {ex}");
        }
        finally
        {
            IsProcessing = false;
        }
    }
    
     // Todo
    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            IsTakingPhoto = true;
            PhotoStatus = "正在启动相机...";

            // 创建临时目录用于保存照片
            var tempDir = Path.Combine(Path.GetTempPath(), "PlantRecognition");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var photoPath = await CapturePhotoWithCamera(tempDir);
        
            if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            {
                SelectedImagePath = photoPath;
                RecognitionResult = "照片拍摄成功，正在自动识别...";
                PhotoStatus = "照片拍摄完成";
            
                // 自动开始识别
                await RecognizePlantAsync();
            }
            else
            {
                RecognitionResult = "拍照失败或用户取消";
                PhotoStatus = "拍照失败";
            }
        }
        catch (Exception ex)
        {
            RecognitionResult = $"拍照失败: {ex.Message}";
            PhotoStatus = "拍照出错";
            Debug.WriteLine($"拍照异常: {ex}");
        }
        finally
        {
            IsTakingPhoto = false;
        }
    }

    private async Task<string> CapturePhotoWithCamera(string outputDirectory)
    {
        try
        {
            // 生成唯一的文件名
            var fileName = $"plant_photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var targetPath = Path.Combine(outputDirectory, fileName);

            PhotoStatus = "正在启动相机...";

            // 方法1: 使用Windows相机应用并自动关闭
            if (OperatingSystem.IsWindows())
            {
                return await CaptureWithWindowsCameraApp(outputDirectory, targetPath);
            }
        
            // 方法2: 备用方案
            return await CaptureWithAlternativeMethod(outputDirectory, targetPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"拍照方法失败: {ex.Message}");
            throw;
        }
    }

    private async Task<string> CaptureWithWindowsCameraApp(string outputDirectory, string targetPath)
    {
        try
        {
            // 获取图片目录
            var picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var cameraRollDir = Path.Combine(picturesDir, "Camera Roll");
        
            // 如果Camera Roll目录不存在，使用图片目录
            var watchDir = Directory.Exists(cameraRollDir) ? cameraRollDir : picturesDir;
        
            // 记录开始前的文件
            var existingFiles = GetImageFiles(watchDir);
            var existingFileSet = new HashSet<string>(existingFiles);

            PhotoStatus = "启动相机应用中...";
        
            // 启动Windows相机应用
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "microsoft.windows.camera:",
                    UseShellExecute = true
                }
            };
        
            process.Start();
        
            PhotoStatus = "相机已启动，请拍照...";

            // 等待新照片文件出现
            var newPhoto = await WaitForNewPhotoFile(watchDir, existingFileSet, TimeSpan.FromSeconds(60));
        
            if (!string.IsNullOrEmpty(newPhoto))
            {
                // 等待文件完全写入
                await WaitForFileReady(newPhoto);
            
                // 复制到目标路径
                File.Copy(newPhoto, targetPath, true);
            
                // 关闭相机应用
                await CloseCameraApp();
            
                PhotoStatus = "照片保存成功";
                return targetPath;
            }
        
            // 如果没有检测到新照片，也尝试关闭相机
            await CloseCameraApp();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows相机应用捕获失败: {ex.Message}");
            // 确保关闭相机应用
            await CloseCameraApp();
            throw;
        }
    }

    private async Task CloseCameraApp()
    {
        try
        {
            // 查找并关闭Windows相机应用
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
                        {
                            process.Kill();
                        }
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
    
    private async Task<string> CaptureWithAlternativeMethod(string outputDirectory, string targetPath)
{
    PhotoStatus = "准备拍照，请使用相机应用...";
    
    // 获取监视目录
    var watchDir = GetPicturesDirectory();
    var existingFiles = GetImageFiles(watchDir);
    var existingFileSet = new HashSet<string>(existingFiles);

    var completionSource = new TaskCompletionSource<string>();
    var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    
    string newPhotoPath = null;

    // 使用文件系统监视器
    using var watcher = new FileSystemWatcher
    {
        Path = watchDir,
        Filter = "*.jpg;*.jpeg;*.png",
        NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size,
        EnableRaisingEvents = true
    };

    void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (!existingFileSet.Contains(e.FullPath) && IsLikelyCameraFile(e.FullPath))
        {
            newPhotoPath = e.FullPath;
            completionSource.TrySetResult(e.FullPath);
        }
    }

    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // 文件内容变化时也检查
        if (!existingFileSet.Contains(e.FullPath) && IsLikelyCameraFile(e.FullPath))
        {
            newPhotoPath = e.FullPath;
            completionSource.TrySetResult(e.FullPath);
        }
    }

    watcher.Created += OnFileCreated;
    watcher.Changed += OnFileChanged;
    
    try
    {
        PhotoStatus = "请使用相机应用拍照...";
        
        // 指导用户
        await ShowCameraInstructions();
        
        // 等待新文件创建或超时
        var completedTask = await Task.WhenAny(
            completionSource.Task,
            Task.Delay(TimeSpan.FromSeconds(60), timeoutToken.Token)
        );

        if (completedTask == completionSource.Task && !string.IsNullOrEmpty(newPhotoPath))
        {
            // 等待文件完全写入
            await WaitForFileReady(newPhotoPath);
            
            File.Copy(newPhotoPath, targetPath, true);
            return targetPath;
        }
        else
        {
            PhotoStatus = "拍照超时";
            return null;
        }
    }
    finally
    {
        watcher.Created -= OnFileCreated;
        watcher.Changed -= OnFileChanged;
        timeoutToken.Dispose();
    }
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
    catch (Exception ex)
    {
        Debug.WriteLine($"获取图片文件列表失败: {ex.Message}");
        return Array.Empty<string>();
    }
}

private async Task<string> WaitForNewPhotoFile(string directory, HashSet<string> existingFiles, TimeSpan timeout)
{
    var startTime = DateTime.Now;
    
    while (DateTime.Now - startTime < timeout)
    {
        try
        {
            var currentFiles = GetImageFiles(directory);
            foreach (var file in currentFiles)
            {
                if (!existingFiles.Contains(file) && IsLikelyCameraFile(file))
                {
                    // 等待文件就绪
                    await WaitForFileReady(file);
                    return file;
                }
            }
            
            await Task.Delay(1000);
            PhotoStatus = $"等待照片... ({DateTime.Now - startTime:mm\\:ss})";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"监控文件时出错: {ex.Message}");
        }
    }
    
    return null;
}

private bool IsLikelyCameraFile(string filePath)
{
    var fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
    
    // 常见的相机应用文件名模式
    var cameraPatterns = new[] 
    {
        "photo", "img", "win_", "camera", "capture", "picture", "wp_", "dsc"
    };
    
    // 检查文件是否来自相机应用
    var isCameraFile = cameraPatterns.Any(pattern => fileName.Contains(pattern)) ||
                      fileName.StartsWith("wp_") || // Windows Phone 模式
                      fileName.StartsWith("dsc") || // 数码相机模式
                      fileName.StartsWith("img_");  // 常见相机命名
    
    if (isCameraFile)
    {
        Debug.WriteLine($"检测到相机文件: {filePath}");
    }
    
    return isCameraFile;
}

private async Task WaitForFileReady(string filePath)
{
    const int maxRetries = 20; // 增加重试次数
    const int delayMs = 500;
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // 如果可以打开文件且文件大小大于0，说明文件就绪
                if (stream.Length > 1024) // 至少1KB，避免读取不完整的文件
                {
                    Debug.WriteLine($"文件就绪: {filePath}, 大小: {stream.Length} 字节");
                    return;
                }
            }
        }
        catch (IOException)
        {
            // 文件可能还在被写入，等待后重试
            Debug.WriteLine($"文件被占用，等待... ({i + 1}/{maxRetries})");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"检查文件时出错: {ex.Message}");
        }
        
        await Task.Delay(delayMs);
    }
    
    throw new TimeoutException($"文件未就绪: {filePath}");
}

private string GetPicturesDirectory()
{
    return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
}

private async Task ShowCameraInstructions()
{
    try
    {
        // 打开相机文件夹，方便用户知道照片保存位置
        var cameraRoll = Path.Combine(GetPicturesDirectory(), "Camera Roll");
        var targetDir = Directory.Exists(cameraRoll) ? cameraRoll : GetPicturesDirectory();
        
        Process.Start(new ProcessStartInfo
        {
            FileName = targetDir,
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"打开图片文件夹失败: {ex.Message}");
    }
    
    // 可以在这里添加更详细的用户指导
    Debug.WriteLine("请使用系统相机应用拍照，照片将自动保存并识别");
    
    
}

    
    
    private async Task<string> GetAccessTokenAsync()
    {
        // 实际API密钥
        const string API_KEY = "LD2JJbOND5SooohHCN44kezi";
        const string SECRET_KEY = "F7xYr8OLo2jAfTu1wvjj0Eiew7tndZ3u";
        
        //创建HTTP客户端
        using var client = new HttpClient();
        //如果客户端在30秒内没有收到响应，就会抛出一个超时异常
        client.Timeout = TimeSpan.FromSeconds(10);
      
        
        //百度的令牌接口需要三个参数grant_type：必须为client_credentials; client_id：应用的API Key; client_secret：应用的Secret Key
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", API_KEY),
            new("client_secret", SECRET_KEY)
        };
        
        try
        {
            //发送POST请求到百度认证接口
            var response = await client.PostAsync(
                "https://aip.baidubce.com/oauth/2.0/token",
                new FormUrlEncodedContent(parameters));
            
            // 检查响应是否成功
            if (response.IsSuccessStatusCode)
            {
                // 读取返回的JSON数据,解析JSON，提取access_token
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                {
                    return tokenElement.GetString();
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"获取访问令牌时出错: {ex.Message}");
        }
    } //获取访问令牌方法
    
    private async Task<PlantRecognitionResult> RecognizePlantApiAsync(string base64Image, string accessToken)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(15);
        
        //准备请求参数（base64格式的图片）
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("image", base64Image),
            new("baike_num", "1")
        };
        
        //构建API URL（包含访问令牌）
        var url = $"https://aip.baidubce.com/rest/2.0/image-classify/v1/plant?access_token={accessToken}";
        //发送POST请求(包含图片和api的url)
        var response = await client.PostAsync(url, new FormUrlEncodedContent(parameters));
        
        if (response.IsSuccessStatusCode)
        {
            //读取并解析返回的JSON数据
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PlantRecognitionResult>(
                json, 
                new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true 
                });
            //Deserialize方法将JSON字符串反序列化为 PlantRecognitionResult 对象。
            return result;
        }
        else
        {
            throw new Exception($"API请求失败: {response.StatusCode}");
        }
    } //调用植物识别API方法
    

    // 辅助方法：获取当前窗口
    private Window GetCurrentWindow()
    {
        // 尝试从应用程序的窗口集合中获取
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
    
}


// API 返回结果类

public class PlantRecognitionResult
{
    public long LogId { get; set; }
    public List<PlantItem> Result { get; set; }
}

public class PlantItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("score")]
    [JsonConverter(typeof(DoubleConverter))] // 添加 JSON 转换器
    public double Score { get; set; }
    
    [JsonPropertyName("baike_info")]  // 明确映射到 JSON 中的 baike_info
    public BaikeInfo BaikeInfo { get; set; }
}

// 添加一个安全的 Double 转换器
public class DoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (double.TryParse(reader.GetString(), out double result))
                {
                    return result;
                }
            }
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}


public class BaikeInfo
{
    
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
}

