using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DailyPlant.Library.Data;
using Xunit;

namespace DailyPlant.UnitTest.Services
{
    public class PlantServiceTest
    {
        private readonly Mock<PlantDbContext> _mockDbContext;
        private readonly PlantService _plantService;

        public PlantServiceTest()
        {
            _mockDbContext = new Mock<PlantDbContext>();
            _plantService = new PlantService(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllPlantsAsync_ShouldReturnAllPlants()
        {
            // Arrange
            var expectedPlants = new List<Plant>
            {
                new Plant { Id = 1, Name = "向日葵", Category = "观花植物" },
                new Plant { Id = 2, Name = "绿萝", Category = "观叶植物" }
            };

            var mockDbSet = TestHelpers.CreateMockDbSet(expectedPlants.AsQueryable());
            _mockDbContext.Setup(db => db.Plants).Returns(mockDbSet.Object);

            // Act
            var result = await _plantService.GetAllPlantsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("向日葵", result[0].Name);
            Assert.Equal("绿萝", result[1].Name);
        }

        [Fact]
        public async Task GetDistinctCategoriesAsync_ShouldReturnDistinctSortedCategories()
        {
            // Arrange
            var plants = new List<Plant>
            {
                new Plant { Id = 1, Name = "向日葵", Category = "观花植物" },
                new Plant { Id = 2, Name = "绿萝", Category = "观叶植物" },
                new Plant { Id = 3, Name = "菊花", Category = "观花植物" },
                new Plant { Id = 4, Name = "龟背竹", Category = "观叶植物" },
                new Plant { Id = 5, Name = "吊兰", Category = null } // Should be filtered out
            };

            var mockDbSet = TestHelpers.CreateMockDbSet(plants.AsQueryable());
            _mockDbContext.Setup(db => db.Plants).Returns(mockDbSet.Object);

            // Act
            var result = await _plantService.GetDistinctCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("观花植物", result[0]);
            Assert.Equal("观叶植物", result[1]);
        }

        [Fact]
        public async Task GetDistinctCategoriesAsync_ShouldReturnEmptyList_WhenNoValidCategories()
        {
            // Arrange
            var plants = new List<Plant>
            {
                new Plant { Id = 1, Name = "Plant1", Category = null },
                new Plant { Id = 2, Name = "Plant2", Category = "" }
            };

            var mockDbSet = TestHelpers.CreateMockDbSet(plants.AsQueryable());
            _mockDbContext.Setup(db => db.Plants).Returns(mockDbSet.Object);

            // Act
            var result = await _plantService.GetDistinctCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("向日葵", "观花植物", 1)]
        [InlineData("", "观花植物", 2)]
        [InlineData("绿萝", "观叶植物", 1)]
        [InlineData("", "全部", 4)] // Should return all plants when category is "全部"
        [InlineData("NonExistent", "观花植物", 0)]
        public async Task GetFilteredPlantsAsync_ShouldFilterCorrectly(string searchText, string selectedCategory,
            int expectedCount)
        {
            // Arrange
            var plants = new List<Plant>
            {
                new Plant { Id = 1, Name = "向日葵", Category = "观花植物" },
                new Plant { Id = 2, Name = "菊花", Category = "观花植物" },
                new Plant { Id = 3, Name = "绿萝", Category = "观叶植物" },
                new Plant { Id = 4, Name = "龟背竹", Category = "观叶植物" }
            };

            var mockDbSet = TestHelpers.CreateMockDbSet(plants.AsQueryable());
            _mockDbContext.Setup(db => db.Plants).Returns(mockDbSet.Object);

            // Act
            var result = await _plantService.GetFilteredPlantsAsync(searchText, selectedCategory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public async Task GetFilteredPlantsAsync_ShouldReturnSortedByName()
        {
            // Arrange
            var plants = new List<Plant>
            {
                new Plant { Id = 1, Name = "绿萝", Category = "观叶植物" },
                new Plant { Id = 2, Name = "向日葵", Category = "观花植物" },
                new Plant { Id = 3, Name = "菊花", Category = "观花植物" }
            };

            var mockDbSet = TestHelpers.CreateMockDbSet(plants.AsQueryable());
            _mockDbContext.Setup(db => db.Plants).Returns(mockDbSet.Object);

            // Act
            var result = await _plantService.GetFilteredPlantsAsync("", "全部");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("菊花", result[0].Name);
            Assert.Equal("绿萝", result[1].Name);
            Assert.Equal("向日葵", result[2].Name);
        }

        // Helper class for creating mock DbSet
        internal static class TestHelpers
        {
            public static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data)
                where T : class
            {
                var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
                mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
                mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
                mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
                mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
                return mockSet;
            }
        }
    }
}