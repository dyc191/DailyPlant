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

        public DailyService(PlantDbContext context)
        {
            _context = context;
        }
        
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

        // 根据季节获取植物
        public async Task<Plant> GetPlantBySeasonAsync(string season)
        {
            
            var allPlants = await _context.Plants.ToListAsync();
            
            var matchingPlants = allPlants
                .Where(p => p.SeasonBloom != null && IsSeasonMatch(p.SeasonBloom, season))
                .ToList();

            if (matchingPlants.Count == 0)
            {
                // 如果没有找到匹配季节的植物，回退到随机选择
                return await GetRandomPlantAsync();
            }

            var random = new Random();
            return matchingPlants[random.Next(matchingPlants.Count)];
        }

        // 判断开花季节是否匹配当前季节
        private bool IsSeasonMatch(string seasonBloom, string currentSeason)
        {
            if (string.IsNullOrEmpty(seasonBloom))
                return false;

            // 处理季节范围（如"夏到冬"）
            if (seasonBloom.Contains('-'))
            {
                return IsSeasonRangeMatch(seasonBloom, currentSeason);
            }

            // 处理多个季节（如"春,秋"）
            if (seasonBloom.Contains("、") || seasonBloom.Contains(","))
            {
                var seasons = seasonBloom.Split(new[] { '、', ',' }, StringSplitOptions.RemoveEmptyEntries);
                return seasons.Any(s => s.Trim() == currentSeason);
            }

            // 处理"四季"情况
            if (seasonBloom.Trim() == "四季")
                return true;

            // 单个季节直接比较
            return seasonBloom.Trim() == currentSeason;
        }

        // 处理季节范围匹配
        private bool IsSeasonRangeMatch(string seasonRange, string currentSeason)
        {
            var parts = seasonRange.Split('-');
            if (parts.Length != 2)
                return false;

            string startSeason = parts[0].Trim();
            string endSeason = parts[1].Trim();

            // 定义季节顺序
            var seasonOrder = new[] { "春", "夏", "秋", "冬" };

            int startIndex = Array.IndexOf(seasonOrder, startSeason);
            int endIndex = Array.IndexOf(seasonOrder, endSeason);
            int currentIndex = Array.IndexOf(seasonOrder, currentSeason);

            if (startIndex == -1 || endIndex == -1 || currentIndex == -1)
                return false;

            // 处理跨年情况（如"冬到春"）
            if (startIndex > endIndex)
            {
                // "冬到春"：包含冬、春，但不包含夏、秋
                // 当当前季节为秋时，不包含"冬到春"
                return currentIndex >= startIndex || currentIndex <= endIndex;
            }
            else
            {
                // 正常范围（如"春到秋"）：包含春、夏、秋
                // 当当前季节为秋时，包含"夏到冬"
                return currentIndex >= startIndex && currentIndex <= endIndex;
            }
        }
    }
}
        
