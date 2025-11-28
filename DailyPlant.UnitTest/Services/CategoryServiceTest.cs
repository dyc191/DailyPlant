using DailyPlant.Library.Services;
using Moq;

namespace DailyPlant.UnitTest.Services;

public class CategoryServiceTest
{
    private readonly Mock<IPlantService> _mockPlantService;
    private readonly CategoryService _categoryService;

    public CategoryServiceTest()
    {
        _mockPlantService = new Mock<IPlantService>();
        _categoryService = new CategoryService(_mockPlantService.Object);
    }

    [Fact]
    public async Task GetCategoriesWithAllOptionAsync_ShouldReturnCategoriesWithAllOption()
    {
        // Arrange
        var categories = new List<string> { "观花植物", "观叶植物", "多肉植物" };
        _mockPlantService.Setup(service => service.GetDistinctCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetCategoriesWithAllOptionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count); // 3 categories + "全部"
        Assert.Equal("全部", result[0]);
        Assert.Equal("观花植物", result[1]);
        Assert.Equal("观叶植物", result[2]);
        Assert.Equal("多肉植物", result[3]);
    }

    [Fact]
    public async Task GetCategoriesWithAllOptionAsync_ShouldReturnOnlyAllOption_WhenNoCategories()
    {
        // Arrange
        var emptyCategories = new List<string>();
        _mockPlantService.Setup(service => service.GetDistinctCategoriesAsync())
            .ReturnsAsync(emptyCategories);

        // Act
        var result = await _categoryService.GetCategoriesWithAllOptionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("全部", result[0]);
    }
}