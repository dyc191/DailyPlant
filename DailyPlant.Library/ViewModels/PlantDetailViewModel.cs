using System.Diagnostics;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;
using DailyPlant.Library.Models;

namespace DailyPlant.Library.ViewModels;

public partial class PlantDetailViewModel : ViewModelBase
{
    private readonly IPlantRecognitionService _plantRecognitionService;
    private readonly IContentNavigationService _contentNavigationService;

    public PlantDetailViewModel(
        IPlantRecognitionService plantRecognitionService,
        IContentNavigationService contentNavigationService)
    {
        _plantRecognitionService = plantRecognitionService;
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
            
                if (plant.BaikeInfo != null)
                {
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
    
    private string FormatTextWithLineBreaks(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "暂无描述信息";
    
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
            PlantImage = await _plantRecognitionService.LoadPlantImageAsync(ImageUrl);
            Debug.WriteLine("植物图片加载完成");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"加载植物图片失败: {ex.Message}");
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
        // 分享功能实现
        await Task.Delay(100);
        Debug.WriteLine("分享功能被调用");
    }
}