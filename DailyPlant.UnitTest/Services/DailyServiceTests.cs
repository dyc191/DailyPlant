using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DailyPlant.Library.Data;
using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyPlant.UnitTest.Services
{
    public class DailyServiceTests : IDisposable
    {
        private readonly PlantDbContext _context;
        private readonly DailyService _service;

        public DailyServiceTests()
        {
            var options = new DbContextOptionsBuilder<PlantDbContext>()
                .UseInMemoryDatabase(databaseName: $"PlantTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new PlantDbContext(options);
            InitializeTestData();
            _service = new DailyService(_context);
        }

        private Plant CreateCompletePlant(int id, string name, string seasonBloom, string seasonSowing = "春")
        {
            return new Plant
            {
                Id = id,
                Name = name,
                SeasonBloom = seasonBloom, // 确保不为 null
                SeasonSowing = seasonSowing,
                Zone = "测试区域",
                Water = "适量",
                Description = "测试描述",
                Image = "test.jpg",
                Category = "测试类别"
            };
        }

        private void InitializeTestData()
        {
            var plants = new List<Plant>
            {
                CreateCompletePlant(1, "玫瑰", "春、夏"),
                CreateCompletePlant(2, "菊花", "秋"),
                CreateCompletePlant(3, "梅花", "冬"),
                CreateCompletePlant(4, "向日葵", "夏"),
                CreateCompletePlant(5, "四季海棠", "四季"),
                CreateCompletePlant(6, "跨年花", "冬-春"),
                CreateCompletePlant(7, "无花期植物", ""), // 使用空字符串而不是 null
                CreateCompletePlant(8, "多季节花", "春、秋")
            };

            _context.Plants.AddRange(plants);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetRandomPlantAsync_WhenPlantsExist_ReturnsPlant()
        {
            // Act
            var result = await _service.GetRandomPlantAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Plant>(result);
            Assert.InRange(result.Id, 1, 8);
        }

        [Fact]
        public async Task GetRandomPlantAsync_WhenNoPlants_ReturnsNull()
        {
            // Arrange
            var emptyOptions = new DbContextOptionsBuilder<PlantDbContext>()
                .UseInMemoryDatabase(databaseName: $"EmptyDb_{Guid.NewGuid()}")
                .Options;
            
            using var emptyContext = new PlantDbContext(emptyOptions);
            var emptyService = new DailyService(emptyContext);

            // Act
            var result = await emptyService.GetRandomPlantAsync();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("春", new[] { 1, 5, 6, 8 })]
        [InlineData("夏", new[] { 1, 4, 5 })]
        [InlineData("秋", new[] { 2, 5, 8 })]
        [InlineData("冬", new[] { 3, 5, 6 })]
        public async Task GetPlantBySeasonAsync_WithMatchingPlants_ReturnsPlantFromExpectedSet(string season, int[] expectedPlantIds)
        {
            // Act - 运行多次以确保覆盖所有可能性
            var results = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                var plant = await _service.GetPlantBySeasonAsync(season);
                results.Add(plant.Id);
            }

            // Assert - 所有结果都应该在预期的植物ID集合中
            var distinctResults = results.Distinct().ToList();
            Assert.All(distinctResults, id => Assert.Contains(id, expectedPlantIds));
        }

        [Theory]
        [InlineData("春")]
        [InlineData("夏")]
        [InlineData("秋")]
        [InlineData("冬")]
        public async Task GetPlantBySeasonAsync_WithNoBloomPlants_FallsBackToRandom(string season)
        {
            // Arrange - 创建只有无花期植物的上下文
            var noBloomOptions = new DbContextOptionsBuilder<PlantDbContext>()
                .UseInMemoryDatabase(databaseName: $"NoBloomDb_{Guid.NewGuid()}")
                .Options;
            
            using var noBloomContext = new PlantDbContext(noBloomOptions);
            
            // 确保所有必需属性都有值，特别是 SeasonBloom 不能为 null
            noBloomContext.Plants.Add(CreateCompletePlant(1, "无花植物1", "")); // 使用空字符串而不是 null
            noBloomContext.Plants.Add(CreateCompletePlant(2, "无花植物2", "")); // 使用空字符串而不是 null
            await noBloomContext.SaveChangesAsync();
            
            var noBloomService = new DailyService(noBloomContext);

            // Act
            var result = await noBloomService.GetPlantBySeasonAsync(season);

            // Assert - 应该回退到随机选择，返回一个植物
            Assert.NotNull(result);
            Assert.IsType<Plant>(result);
        }

        [Fact]
        public async Task GetPlantBySeasonAsync_WithCrossYearRange_MatchesCorrectSeasons()
        {
            // Act & Assert - 跨年花应该在冬季和春季都匹配
            var winterResults = new List<int>();
            var springResults = new List<int>();
            
            for (int i = 0; i < 10; i++)
            {
                winterResults.Add((await _service.GetPlantBySeasonAsync("冬")).Id);
                springResults.Add((await _service.GetPlantBySeasonAsync("春")).Id);
            }

            // 跨年花(Id=6)应该出现在冬季和春季的结果中
            Assert.Contains(6, winterResults.Distinct());
            Assert.Contains(6, springResults.Distinct());
        }

        [Fact]
        public async Task GetPlantBySeasonAsync_WithFourSeasonsPlant_MatchesAllSeasons()
        {
            // 为每个季节创建一个独立的数据库，只包含四季海棠
            var seasons = new[] { "春", "夏", "秋", "冬" };
            foreach (var season in seasons)
            {
                var options = new DbContextOptionsBuilder<PlantDbContext>()
                    .UseInMemoryDatabase(databaseName: $"FourSeasonsDb_{season}_{Guid.NewGuid()}")
                    .Options;

                using var context = new PlantDbContext(options);
                context.Plants.Add(CreateCompletePlant(1, "四季海棠", "四季"));
                await context.SaveChangesAsync();

                var service = new DailyService(context);
                var result = await service.GetPlantBySeasonAsync(season);

                Assert.NotNull(result);
                Assert.Equal(1, result.Id);
                Assert.Equal("四季海棠", result.Name);
            }
        }

        [Fact]
        public async Task GetPlantBySeasonAsync_WithMultipleSeasons_MatchesCorrectly()
        {
            // Act & Assert - 测试多个季节的情况
            var springResults = new List<int>();
            var autumnResults = new List<int>();
            
            for (int i = 0; i < 20; i++)
            {
                springResults.Add((await _service.GetPlantBySeasonAsync("春")).Id);
                autumnResults.Add((await _service.GetPlantBySeasonAsync("秋")).Id);
            }

            // 多季节花(Id=8)应该在春季和秋季都出现
            Assert.Contains(8, springResults.Distinct());
            Assert.Contains(8, autumnResults.Distinct());
        }

        [Fact]
        public async Task GetPlantBySeasonAsync_WithSingleSeason_ReturnsCorrectPlant()
        {
            // Act - 多次运行以获取菊花(只在秋季开花)
            var results = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                var plant = await _service.GetPlantBySeasonAsync("秋");
                results.Add(plant.Id);
            }

            // Assert - 菊花(Id=2)应该出现在结果中
            Assert.Contains(2, results.Distinct());
            
            // 同时验证不应该出现在其他季节
            var springResults = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                var plant = await _service.GetPlantBySeasonAsync("春");
                springResults.Add(plant.Id);
            }
            
            // 菊花不应该出现在春季结果中
            Assert.DoesNotContain(2, springResults.Distinct());
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}