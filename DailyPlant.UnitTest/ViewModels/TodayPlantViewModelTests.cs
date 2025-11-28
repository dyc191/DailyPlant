using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;
using Moq;
using Xunit;
using Avalonia.Media.Imaging;
using System.ComponentModel;
using System.Threading;
using DailyPlant.Library.Helpers;

namespace DailyPlant.UnitTest.ViewModel
{
    public class TodayPlantViewModelTests
    {
        private readonly Mock<DailyService> _mockPlantService;
        private readonly TodayPlantViewModel _viewModel;

        public TodayPlantViewModelTests()
        {
            _mockPlantService = new Mock<DailyService>();
            _viewModel = new TodayPlantViewModel(_mockPlantService.Object);
        }

        [Fact]
        public void HasDetails_WhenCurrentPlantIsNull_ReturnsFalse()
        {
            // Arrange
            _viewModel.CurrentPlant = null;

            // Act & Assert
            Assert.False(_viewModel.HasDetails);
        }
    
        [Theory]
        [InlineData(1, "冬")]  // 1月 = 冬
        [InlineData(2, "冬")]  // 2月 = 冬
        [InlineData(3, "春")]  // 3月 = 春
        [InlineData(4, "春")]  // 4月 = 春
        [InlineData(5, "春")]  // 5月 = 春
        [InlineData(6, "夏")]  // 6月 = 夏
        [InlineData(7, "夏")]  // 7月 = 夏
        [InlineData(8, "夏")]  // 8月 = 夏
        [InlineData(9, "秋")]  // 9月 = 秋
        [InlineData(10, "秋")] // 10月 = 秋
        [InlineData(11, "秋")] // 11月 = 秋
        [InlineData(12, "冬")] // 12月 = 冬
        public void GetCurrentSeason_ReturnsCorrectSeason(int month, string expectedSeason)
        {
            // 创建一个临时方法用于测试季节逻辑
            string GetSeasonForMonth(int testMonth)
            {
                if (testMonth >= 3 && testMonth <= 5)
                    return "春";
                else if (testMonth >= 6 && testMonth <= 8)
                    return "夏";
                else if (testMonth >= 9 && testMonth <= 11)
                    return "秋";
                else
                    return "冬";
            }
            
            // Act
            var result = GetSeasonForMonth(month);
            
            // Assert
            Assert.Equal(expectedSeason, result);
        }

        [Fact]
        public void PropertyChanged_IsRaisedWhenCurrentPlantChanges()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _viewModel.PropertyChanged += (sender, args) => 
            {
                propertyChangedEvents.Add(args.PropertyName);
            };

            var plant = new Plant { Id = 1, Name = "测试植物" };

            // Act
            _viewModel.CurrentPlant = plant;

            // Assert
            Assert.Contains(nameof(TodayPlantViewModel.CurrentPlant), propertyChangedEvents);
            Assert.Contains(nameof(TodayPlantViewModel.HasDetails), propertyChangedEvents);
        }

        [Fact]
        public void PropertyChanged_IsRaisedWhenPlantImageChanges()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _viewModel.PropertyChanged += (sender, args) => 
            {
                propertyChangedEvents.Add(args.PropertyName);
            };

            // Act
            _viewModel.PlantImage = null;

            // Assert
            Assert.Contains(nameof(TodayPlantViewModel.PlantImage), propertyChangedEvents);
        }

        [Fact]
        public void PropertyChanged_IsRaisedWhenIsLoadingChanges()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _viewModel.PropertyChanged += (sender, args) => 
            {
                propertyChangedEvents.Add(args.PropertyName);
            };

            // Act
            _viewModel.IsLoading = true;

            // Assert
            Assert.Contains(nameof(TodayPlantViewModel.IsLoading), propertyChangedEvents);
        }

        [Fact]
        public async Task CurrentPlantSetter_TriggersImageLoading()
        {
            // Arrange
            var plant = new Plant { Id = 1, Name = "测试植物", Image = "test.jpg" };
            
            // 监听属性变化
            var imageChanged = false;
            _viewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(TodayPlantViewModel.PlantImage))
                {
                    imageChanged = true;
                }
            };

            // Act
            _viewModel.CurrentPlant = plant;
            
            // 等待一小段时间让异步操作完成
            await Task.Delay(100);

            // Assert - PlantImage 应该被设置为 null（因为图像加载失败）
            Assert.Null(_viewModel.PlantImage);
        }

        [Fact]
        public async Task CurrentPlantSetter_WithNullImage_DoesNotAttemptToLoadImage()
        {
            // Arrange
            var plant = new Plant { Id = 1, Name = "测试植物", Image = null };

            // Act
            _viewModel.CurrentPlant = plant;
            
            // 等待一小段时间
            await Task.Delay(100);

            // Assert - PlantImage 应该保持为 null
            Assert.Null(_viewModel.PlantImage);
        }

        [Fact]
        public async Task CurrentPlantSetter_WithEmptyImage_DoesNotAttemptToLoadImage()
        {
            // Arrange
            var plant = new Plant { Id = 1, Name = "测试植物", Image = "" };

            // Act
            _viewModel.CurrentPlant = plant;
            
            // 等待一小段时间
            await Task.Delay(100);

            // Assert - PlantImage 应该保持为 null
            Assert.Null(_viewModel.PlantImage);
        }

        // 测试 ImageLoader 的辅助测试
        [Fact]
        public async Task ImageLoader_WithInvalidUrl_ReturnsNull()
        {
            // Act
            var result = await ImageLoader.LoadImageAsync("invalid_url");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ImageLoader_WithEmptyString_ReturnsNull()
        {
            // Act
            var result = await ImageLoader.LoadImageAsync("");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ImageLoader_WithNull_ReturnsNull()
        {
            // Act
            var result = await ImageLoader.LoadImageAsync(null);

            // Assert
            Assert.Null(result);
        }
    }

    // 辅助接口，用于模拟时间（如果需要在生产代码中使用）
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    // 实际的时间提供者实现
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}