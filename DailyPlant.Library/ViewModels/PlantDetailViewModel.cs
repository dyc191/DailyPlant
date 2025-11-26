using System.Diagnostics;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels;

public partial class PlantDetailViewModel : ViewModelBase
{
    private readonly IContentNavigationService _contentNavigationService;

    public PlantDetailViewModel(IContentNavigationService contentNavigationService)
    {
        _contentNavigationService = contentNavigationService;
        Debug.WriteLine("PlantDetailViewModel 构造函数被调用");
    }

    [ObservableProperty]
    private string _plantName = "未知植物";

    [ObservableProperty]
    private double _score;

    [ObservableProperty]
    private string _description = "暂无描述信息";

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private Bitmap _plantImage;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _confidenceText = "可信度: 0%";

    // 在 PlantDetailViewModel.cs 的 SetParameter 方法中修改
    public override void SetParameter(object parameter)
    {
        Debug.WriteLine($"SetParameter 被调用，参数类型: {parameter?.GetType().Name}");
    
        if (parameter is PlantRecognitionResult plantResult)
        {
            Debug.WriteLine($"识别结果包含 {plantResult.Result?.Count ?? 0} 个植物");
        
            if (plantResult.Result?.Count > 0)
            {
                var plant = plantResult.Result[0];
                PlantName = plant.Name ?? "未知植物";
                Score = plant.Score;
                ConfidenceText = $"识别可信度: {plant.Score * 100:F1}%";
            
                // 检查百科信息
                if (plant.BaikeInfo != null)
                {
                    // 确保文本有适当的换行
                    Description = FormatTextWithLineBreaks(plant.BaikeInfo.Description ?? "暂无详细的百科描述信息。");
                    ImageUrl = plant.BaikeInfo.ImageUrl ?? string.Empty;
                    Debug.WriteLine($"设置描述，长度: {Description.Length}");
                }
                else
                {
                    Description = "该植物暂无百科信息。";
                    ImageUrl = string.Empty;
                    Debug.WriteLine("没有百科信息");
                }

                // 异步加载图片
                _ = LoadPlantImageAsync();
            }
            else
            {
                Description = "未识别到任何植物。";
                IsLoading = false;
            }
        }
        else
        {
            Debug.WriteLine("参数不是 PlantRecognitionResult 类型");
            Description = "数据加载失败。";
            IsLoading = false;
        }
    }
    
    
    // 添加文本格式化方法
    private string FormatTextWithLineBreaks(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "暂无描述信息";
    
        // 在句号后添加换行，使文本更易读
        return text.Replace("。", "。\n");
    }

    private async Task LoadPlantImageAsync()
    {
        Debug.WriteLine($"开始加载图片，URL: {ImageUrl}");
        
        if (string.IsNullOrEmpty(ImageUrl))
        {
            Debug.WriteLine("图片URL为空，跳过加载");
            IsLoading = false;
            return;
        }

        try
        {
            using var httpClient = new HttpClient();
            // 设置用户代理，避免被某些网站拒绝
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            Debug.WriteLine($"正在下载图片: {ImageUrl}");
            var imageBytes = await httpClient.GetByteArrayAsync((string?)ImageUrl);
            Debug.WriteLine($"图片下载完成，大小: {imageBytes.Length} 字节");
            
            using var memoryStream = new MemoryStream(imageBytes);
            PlantImage = new Bitmap(memoryStream);
            Debug.WriteLine("Bitmap 创建成功");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"加载植物图片失败: {ex.Message}");
            // 设置一个默认图片或保持为 null
            PlantImage = null;
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine($"图片加载完成，IsLoading = {IsLoading}");
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        Debug.WriteLine("返回按钮被点击");
        _contentNavigationService.NavigateTo(ContentNavigationConstant.TakePhotoView);
    }

    [RelayCommand]
    private async Task ShareInfo()
    {
        await Task.Delay(100);
    }
}