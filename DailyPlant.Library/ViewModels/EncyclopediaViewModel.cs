// EncyclopediaViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Models;
using DailyPlant.Library.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace DailyPlant.Library.ViewModels
{
    public partial class EncyclopediaViewModel : ViewModelBase
    {
        private readonly PlantDbContext _dbContext;
        private List<Plant> _allPlants = new();

        [ObservableProperty]
        private ObservableCollection<Plant> _plants = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "全部";

        [ObservableProperty]
        private ObservableCollection<string> _categories = new()
        {
            "全部"
        };

        [ObservableProperty]
        private bool _isLoading = false;

        public EncyclopediaViewModel()
        {
            _dbContext = new PlantDbContext();
            LoadPlants();
        }

        private async void LoadPlants()
        {
            try
            {
                IsLoading = true;
                
                // 模拟异步加载（如果是实际数据库操作，这里应该是异步的）
                await Task.Run(() =>
                {
                    _allPlants = _dbContext.Plants.ToList();
                });
                
                // 获取所有分类
                var distinctCategories = _allPlants
                    .Select(p => p.Category)
                    .Distinct()
                    .Where(c => !string.IsNullOrEmpty(c))
                    .OrderBy(c => c)
                    .ToList();
                
                Categories.Clear();
                Categories.Add("全部");
                foreach (var category in distinctCategories)
                {
                    Categories.Add(category);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                // 处理异常，可以记录日志或显示错误信息
                System.Diagnostics.Debug.WriteLine($"加载植物数据失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilters();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            ApplyFilters();
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedCategoryChanged(string value)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredPlants = _allPlants.AsEnumerable();

            // 按分类筛选
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "全部")
            {
                filteredPlants = filteredPlants.Where(p => p.Category == SelectedCategory);
            }

            // 按搜索文本筛选
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filteredPlants = filteredPlants.Where(p => 
                    (p.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Description?.ToLower().Contains(searchLower) ?? false) ||
                    (p.SeasonSowing?.ToLower().Contains(searchLower) ?? false) ||
                    (p.SeasonBloom?.ToLower().Contains(searchLower) ?? false));
            }

            Plants.Clear();
            foreach (var plant in filteredPlants.OrderBy(p => p.Name))
            {
                Plants.Add(plant);
            }
        }
    }
}