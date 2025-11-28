using DailyPlant.Library.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DailyPlant.Library.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IPlantService _plantService;

        public CategoryService(IPlantService plantService)
        {
            _plantService = plantService;
        }

        public async Task<List<string>> GetCategoriesWithAllOptionAsync()
        {
            var categories = await _plantService.GetDistinctCategoriesAsync();
            
            var result = new List<string> { "全部" };
            result.AddRange(categories);
            
            return result;
        }
    }
}