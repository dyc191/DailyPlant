using DailyPlant.Library.Models;
using DailyPlant.Library.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DailyPlant.Library.Services
{
    public class DailyService
    {
        private readonly PlantDbContext _context;

        public DailyService()
        {
            _context = new PlantDbContext();
        }
        
        // 获取随机植物
        public async Task<Plant> GetRandomPlantAsync()
        {
            
            var totalCount = await _context.Plants.CountAsync();
            if (totalCount == 0)
                return null;

            var random = new Random();
            var skip = random.Next(0, totalCount);
            
            return await _context.Plants
                .Skip(skip)
                .FirstOrDefaultAsync();
            
        }
    }
}
