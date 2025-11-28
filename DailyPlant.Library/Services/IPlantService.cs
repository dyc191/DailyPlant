using DailyPlant.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyPlant.Library.Services
{
    public interface IPlantService
    {
        Task<List<Plant>> GetAllPlantsAsync();
        Task<List<string>> GetDistinctCategoriesAsync();
        Task<List<Plant>> GetFilteredPlantsAsync(string searchText, string selectedCategory);
    }
}