using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;
using Moq;
using Xunit;

namespace DailyPlant.UnitTest.ViewModels;

public class PlantDetailViewModelTests
{
    private readonly Mock<IPlantRecognitionService> _mockPlantRecognitionService;
    private readonly Mock<IContentNavigationService> _mockContentNavigationService;
    private readonly PlantDetailViewModel _viewModel;

    public PlantDetailViewModelTests()
    {
        _mockPlantRecognitionService = new Mock<IPlantRecognitionService>();
        _mockContentNavigationService = new Mock<IContentNavigationService>();
        _viewModel = new PlantDetailViewModel(
            _mockPlantRecognitionService.Object,
            _mockContentNavigationService.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_InitializesProperties()
    {
        // Assert
        Assert.NotNull(_viewModel);
        Assert.Equal("未知植物", _viewModel.PlantName);
        Assert.Equal(0, _viewModel.Score);
        Assert.Equal("暂无描述信息", _viewModel.Description);
        Assert.True(_viewModel.IsLoading);
    }

    [Fact]
    public void SetParameter_WithValidPlantRecognitionResult_SetsPropertiesCorrectly()
    {
        // Arrange
        var plantResult = new PlantRecognitionResult
        {
            Result = new List<PlantItem>
            {
                new PlantItem
                {
                    Name = "玫瑰",
                    Score = 0.89,
                    BaikeInfo = new BaikeInfo
                    {
                        Description = "玫瑰是一种美丽的花卉",
                        ImageUrl = "http://example.com/rose.jpg"
                    }
                }
            }
        };

        // Act
        _viewModel.SetParameter(plantResult);

        // Assert
        Assert.Equal("玫瑰", _viewModel.PlantName);
        Assert.Equal(0.89, _viewModel.Score);
        Assert.Contains("玫瑰是一种美丽的花卉", _viewModel.Description);
        Assert.Equal("http://example.com/rose.jpg", _viewModel.ImageUrl);
    }

    [Fact]
    public void SetParameter_WithPlantResultWithoutBaikeInfo_SetsDefaultValues()
    {
        // Arrange
        var plantResult = new PlantRecognitionResult
        {
            Result = new List<PlantItem>
            {
                new PlantItem { Name = "未知植物", Score = 0.75 }
            }
        };

        // Act
        _viewModel.SetParameter(plantResult);

        // Assert
        Assert.Equal("未知植物", _viewModel.PlantName);
        Assert.Equal(0.75, _viewModel.Score);
        Assert.Equal("该植物暂无百科信息。", _viewModel.Description);
    }

    [Fact]
    public void SetParameter_WithEmptyPlantResult_SetsErrorMessage()
    {
        // Arrange
        var plantResult = new PlantRecognitionResult { Result = new List<PlantItem>() };

        // Act
        _viewModel.SetParameter(plantResult);

        // Assert
        Assert.Equal("未识别到任何植物。", _viewModel.Description);
    }

    [Fact]
    public void GoBackCommand_WhenExecuted_NavigatesToTakePhotoView()
    {
        // Act
        _viewModel.GoBackCommand.Execute(null);

        // Assert
        _mockContentNavigationService.Verify(
            x => x.NavigateTo(ContentNavigationConstant.TakePhotoView,null),
            Times.Once);
    }

    [Fact]
    public async Task ShareInfoCommand_WhenExecuted_CompletesWithoutException()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _viewModel.ShareInfoCommand.ExecuteAsync(null));
        Assert.Null(exception);
    }

    [Fact]
    public void Commands_AreInitialized()
    {
        // Assert
        Assert.NotNull(_viewModel.GoBackCommand);
        Assert.NotNull(_viewModel.ShareInfoCommand);
    }

    [Fact]
    public void PropertyChanged_WhenPropertiesChange_RaisesEvents()
    {
        // Arrange
        var changedProperties = new List<string>();
        _viewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

        // Act
        _viewModel.PlantName = "新植物";
        _viewModel.Score = 0.95;
        _viewModel.Description = "新描述";

        // Assert
        Assert.Contains(nameof(PlantDetailViewModel.PlantName), changedProperties);
        Assert.Contains(nameof(PlantDetailViewModel.Score), changedProperties);
        Assert.Contains(nameof(PlantDetailViewModel.Description), changedProperties);
    }
}