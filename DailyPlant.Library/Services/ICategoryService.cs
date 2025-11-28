using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyPlant.Library.Services
{
    public interface ICategoryService
    {
        Task<List<string>> GetCategoriesWithAllOptionAsync();
    }
}