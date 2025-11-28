using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.ViewModels;

public partial class TakePhotoViewModel : ViewModelBase
{
    private readonly IPlantRecognitionService _plantRecognitionService;
    private readonly IContentNavigationService _contentNavigationService;

    [ObservableProperty]
    private string _selectedImagePath = string.Empty;

    [ObservableProperty]
    private string _recognitionResult = "请选择图片进行植物识别";

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private PlantRecognitionResult _plantResult;

    [ObservableProperty]
    private bool _isTakingPhoto = false;

    [ObservableProperty]
    private string _photoStatus = "准备拍照";

    public TakePhotoViewModel(
        IPlantRecognitionService plantRecognitionService,
        IContentNavigationService contentNavigationService)
    {
        _plantRecognitionService = plantRecognitionService;
        _contentNavigationService = contentNavigationService;
        Debug.WriteLine("TakePhotoViewModel 构造函数被调用");
    }

    [RelayCommand]
    private async Task SelectImageAsync()
    {
        try
        {
            var window = GetCurrentWindow();
            if (window == null) return;
            
            var topLevel = TopLevel.GetTopLevel(window);
            if (topLevel == null) return;
            
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
            
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            
            if (files.Count > 0 && files[0] is IStorageFile file)
            {
                SelectedImagePath = file.Path.LocalPath;
                RecognitionResult = "图片已选择，点击识别按钮进行识别";
                
                // 验证图片
                var isValid = await _plantRecognitionService.ValidateImageAsync(SelectedImagePath);
                if (!isValid)
                {
                    RecognitionResult = "选择的图片文件无效，请重新选择";
                    SelectedImagePath = string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            RecognitionResult = $"选择图片失败: {ex.Message}";
            Debug.WriteLine($"选择图片异常: {ex}");
        }
    }

    [RelayCommand]
    private async Task RecognizePlantAsync()
    {
        if (string.IsNullOrEmpty(SelectedImagePath) || !File.Exists(SelectedImagePath))
        {
            RecognitionResult = "请先选择有效的图片";
            return;
        }

        try
        {
            IsProcessing = true;
            RecognitionResult = "识别中...";

            Debug.WriteLine($"开始识别植物: {SelectedImagePath}");
            var result = await _plantRecognitionService.RecognizePlantAsync(SelectedImagePath);

            if (result?.Result?.Count > 0)
            {
                var plant = result.Result[0];
                Debug.WriteLine($"识别到植物: {plant.Name}, 分数: {plant.Score}");
                
                // 跳转到详情页面
                _contentNavigationService.NavigateTo(ContentNavigationConstant.PlantDetailView, result);
                RecognitionResult = "识别成功，跳转中...";
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

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            IsTakingPhoto = true;
            PhotoStatus = "正在启动相机...";

            var photoPath = await _plantRecognitionService.CapturePhotoAsync();
        
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

    private Window GetCurrentWindow()
    {
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}