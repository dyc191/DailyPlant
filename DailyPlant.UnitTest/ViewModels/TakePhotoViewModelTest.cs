using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;
using Moq;
using Xunit;

namespace DailyPlant.UnitTest.ViewModels;

public class TakePhotoViewModelTest
{
    private readonly TakePhotoViewModel _viewModel;

    public TakePhotoViewModelTest()
    {
        var mockPlantRecognitionService = new Mock<IPlantRecognitionService>();
        var mockContentNavigationService = new Mock<IContentNavigationService>();
        _viewModel = new TakePhotoViewModel(
            mockPlantRecognitionService.Object,
            mockContentNavigationService.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_InitializesProperties()
    {
        // Assert
        Assert.NotNull(_viewModel);
        Assert.Equal("请选择图片进行植物识别", _viewModel.RecognitionResult);
        Assert.False(_viewModel.IsProcessing);
        Assert.False(_viewModel.IsTakingPhoto);
    }

    [Fact]
    public void Commands_AreInitialized()
    {
        // Assert
        Assert.NotNull(_viewModel.SelectImageCommand);
        Assert.NotNull(_viewModel.RecognizePlantCommand);
        Assert.NotNull(_viewModel.TakePhotoCommand);
    }

    [Fact]
    public void PropertyChanged_WhenPropertiesChange_RaisesEvents()
    {
        // Arrange
        var changedProperties = new List<string>();
        _viewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

        // Act
        _viewModel.SelectedImagePath = "new_path.jpg";
        _viewModel.RecognitionResult = "新的结果";

        // Assert
        Assert.Contains(nameof(TakePhotoViewModel.SelectedImagePath), changedProperties);
        Assert.Contains(nameof(TakePhotoViewModel.RecognitionResult), changedProperties);
    }

    [Fact]
    public void SelectedImagePath_WhenSet_UpdatesProperty()
    {
        // Act
        _viewModel.SelectedImagePath = "test.jpg";

        // Assert
        Assert.Equal("test.jpg", _viewModel.SelectedImagePath);
    }

    [Fact]
    public void RecognitionResult_WhenSet_UpdatesProperty()
    {
        // Act
        _viewModel.RecognitionResult = "测试结果";

        // Assert
        Assert.Equal("测试结果", _viewModel.RecognitionResult);
    }

    [Fact]
    public void IsProcessing_WhenSet_UpdatesProperty()
    {
        // Act
        _viewModel.IsProcessing = true;

        // Assert
        Assert.True(_viewModel.IsProcessing);
    }

    [Fact]
    public void IsTakingPhoto_WhenSet_UpdatesProperty()
    {
        // Act
        _viewModel.IsTakingPhoto = true;

        // Assert
        Assert.True(_viewModel.IsTakingPhoto);
    }
}