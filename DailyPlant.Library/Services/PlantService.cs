using DailyPlant.Library.Data;
using DailyPlant.Library.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DailyPlant.Library.Services
{
    public class PlantService : IPlantService
    {
        private readonly PlantDbContext _dbContext;

        public PlantService(PlantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            // 模拟异步操作
            return await Task.Run(() => _dbContext.Plants.ToList());
        }

        public async Task<List<string>> GetDistinctCategoriesAsync()
        {
            var plants = await GetAllPlantsAsync();
            
            return plants
                .Select(p => p.Category)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .OrderBy(c => c)
                .ToList();
        }

        public async Task<List<Plant>> GetFilteredPlantsAsync(string searchText, string selectedCategory)
        {
            var allPlants = await GetAllPlantsAsync();
            var filteredPlants = allPlants.AsEnumerable();

            // 按分类筛选
            if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "全部")
            {
                filteredPlants = filteredPlants.Where(p => p.Category == selectedCategory);
            }

            // 按搜索文本筛选
            if (!string.IsNullOrEmpty(searchText))
            {
                var searchLower = searchText.ToLower();
                filteredPlants = filteredPlants.Where(p => 
                    p.Name?.ToLower().Contains(searchLower) ?? false);
            }

            return filteredPlants.OrderBy(p => p.Name).ToList();
        }
    }
}