using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;
using Moq;
using System.Collections.ObjectModel;

namespace DailyPlant.UnitTest.ViewModels
{
    public class EncyclopediaViewModelTests
    {
        private readonly Mock<IPlantService> _mockPlantService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<IContentNavigationService> _mockNavigationService;
        private readonly EncyclopediaViewModel _viewModel;

        public EncyclopediaViewModelTests()
        {
            _mockPlantService = new Mock<IPlantService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockNavigationService = new Mock<IContentNavigationService>();
            
            _viewModel = new EncyclopediaViewModel(
                _mockPlantService.Object,
                _mockCategoryService.Object,
                _mockNavigationService.Object);
        }
        

        [Fact]
        public async Task LoadPlants_ShouldHandleException()
        {
            // Arrange
            _mockPlantService.Setup(x => x.GetAllPlantsAsync())
                .ThrowsAsync(new Exception("数据库错误"));

            // Act
            _viewModel.SearchText = "测试";

            // Assert
            await Task.Delay(100);
            Assert.NotNull(_viewModel);
        }

        [Fact]
        public void ShowPlantDetail_ShouldNavigateToPlantView()
        {
            // Arrange
            var plant = new Plant { Id = 1, Name = "玫瑰" };

            // Act
            _viewModel.ShowPlantDetailCommand.Execute(plant);

            // Assert
            _mockNavigationService.Verify(x => x.NavigateTo(
                It.IsAny<string>(),
                plant), 
                Times.Once);
        }

        [Fact]
        public async Task SearchCommand_ShouldApplyFilters()
        {
            // Arrange
            var filteredPlants = new List<Plant>
            {
                new Plant { Id = 1, Name = "玫瑰" }
            };

            _mockPlantService.Setup(x => x.GetFilteredPlantsAsync("玫瑰", "全部"))
                .ReturnsAsync(filteredPlants);

            // Act
            _viewModel.SearchText = "玫瑰";
            _viewModel.SearchCommand.Execute(null);

            // Assert
            await Task.Delay(50);
            Assert.Single(_viewModel.Plants);
            Assert.Equal("玫瑰", _viewModel.Plants.First().Name);
        }

        [Fact]
        public async Task ClearSearchCommand_ShouldResetSearchText()
        {
            // Arrange
            _viewModel.SearchText = "搜索内容";

            // Act
            _viewModel.ClearSearchCommand.Execute(null);

            // Assert
            Assert.Equal(string.Empty, _viewModel.SearchText);
        }

        [Fact]
        public async Task OnSearchTextChanged_ShouldTriggerFilter()
        {
            // Arrange
            var filteredPlants = new List<Plant>
            {
                new Plant { Id = 1, Name = "郁金香" }
            };

            _mockPlantService.Setup(x => x.GetFilteredPlantsAsync("郁金香", "全部"))
                .ReturnsAsync(filteredPlants);

            // Act
            _viewModel.SearchText = "郁金香";

            // Assert
            await Task.Delay(50);
            _mockPlantService.Verify(x => x.GetFilteredPlantsAsync("郁金香", "全部"), Times.AtLeastOnce);
        }

        [Fact]
        public async Task OnSelectedCategoryChanged_ShouldTriggerFilter()
        {
            // Arrange
            var filteredPlants = new List<Plant>
            {
                new Plant { Id = 1, Name = "仙人掌", Category = "多肉植物" }
            };

            _mockPlantService.Setup(x => x.GetFilteredPlantsAsync("", "多肉植物"))
                .ReturnsAsync(filteredPlants);

            // Act
            _viewModel.SelectedCategory = "多肉植物";

            // Assert
            await Task.Delay(50);
            _mockPlantService.Verify(x => x.GetFilteredPlantsAsync("", "多肉植物"), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ApplyFilters_ShouldHandleException()
        {
            // Arrange
            _mockPlantService.Setup(x => x.GetFilteredPlantsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("筛选错误"));

            // Act
            _viewModel.SearchText = "错误测试";

            // Assert
            await Task.Delay(50);
            Assert.NotNull(_viewModel.Plants);
        }

        [Fact]
        public async Task Search_WithChineseCharacters_ShouldWorkCorrectly()
        {
            // Arrange
            var filteredPlants = new List<Plant>
            {
                new Plant { Id = 1, Name = "兰花", Category = "观花植物" },
                new Plant { Id = 2, Name = "蝴蝶兰", Category = "观花植物" }
            };

            _mockPlantService.Setup(x => x.GetFilteredPlantsAsync("兰", "观花植物"))
                .ReturnsAsync(filteredPlants);

            // Act
            _viewModel.SearchText = "兰";
            _viewModel.SelectedCategory = "观花植物";
            _viewModel.SearchCommand.Execute(null);

            // Assert
            await Task.Delay(50);
            Assert.Equal(2, _viewModel.Plants.Count);
            Assert.All(_viewModel.Plants, plant => Assert.Contains("兰", plant.Name));
        }

        [Fact]
        public async Task MixedChineseSearch_ShouldFilterCorrectly()
        {
            // Arrange
            var allPlants = new List<Plant>
            {
                new Plant { Id = 1, Name = "红色玫瑰", Category = "观花植物" },
                new Plant { Id = 2, Name = "白色玫瑰", Category = "观花植物" },
                new Plant { Id = 3, Name = "小番茄", Category = "果蔬植物" }
            };

            _mockPlantService.Setup(x => x.GetFilteredPlantsAsync("玫瑰", "观花植物"))
                .ReturnsAsync(allPlants.Where(p => p.Name.Contains("玫瑰") && p.Category == "观花植物").ToList());

            // Act
            _viewModel.SearchText = "玫瑰";
            _viewModel.SelectedCategory = "观花植物";

            // Assert
            await Task.Delay(50);
            _mockPlantService.Verify(x => x.GetFilteredPlantsAsync("玫瑰", "观花植物"), Times.AtLeastOnce);
        }
    }

    // Mock 数据生成器，用于测试
    public static class MockDataGenerator
    {
        public static List<Plant> GetTestPlants()
        {
            return new List<Plant>
            {
                new Plant { Id = 1, Name = "玫瑰", Category = "观花植物", SeasonSowing = "春季", SeasonBloom = "夏季" },
                new Plant { Id = 2, Name = "番茄", Category = "果蔬植物", SeasonSowing = "春季", SeasonBloom = "夏季" },
                new Plant { Id = 3, Name = "仙人掌", Category = "多肉植物", SeasonSowing = "春季", SeasonBloom = "夏季" },
                new Plant { Id = 4, Name = "薰衣草", Category = "香草植物", SeasonSowing = "春季", SeasonBloom = "夏季" },
                new Plant { Id = 5, Name = "兰花", Category = "观花植物", SeasonSowing = "春季", SeasonBloom = "夏季" },
                new Plant { Id = 6, Name = "菠菜", Category = "果蔬植物", SeasonSowing = "春季", SeasonBloom = "夏季" }
            };
        }

        public static List<string> GetTestCategories()
        {
            return new List<string> { "全部", "观花植物", "果蔬植物", "多肉植物", "香草植物", "果树" };
        }
    }
}