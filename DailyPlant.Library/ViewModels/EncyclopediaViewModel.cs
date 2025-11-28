using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace DailyPlant.Library.ViewModels
{
    public partial class EncyclopediaViewModel : ViewModelBase
    {
        private readonly IPlantService _plantService;
        private readonly ICategoryService _categoryService;
        private readonly IContentNavigationService _navigationService;
        
        private List<Plant> _allPlants = new();

        [ObservableProperty]
        private ObservableCollection<Plant> _plants = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "全部";

        [ObservableProperty]
        private ObservableCollection<string> _categories = new() { "全部" };

        [ObservableProperty]
        private bool _isLoading = false;

        public EncyclopediaViewModel(
            IPlantService plantService,
            ICategoryService categoryService,
            IContentNavigationService navigationService)
        {
            _plantService = plantService;
            _categoryService = categoryService;
            _navigationService = navigationService;
            LoadPlants();
        }
        
        [RelayCommand]
        private void ShowPlantDetail(Plant plant)
        {
            _navigationService.NavigateTo(ContentNavigationConstant.PlantView, plant);
        }

        private async void LoadPlants()
        {
            try
            {
                IsLoading = true;
                
                // 使用服务获取数据
                _allPlants = await _plantService.GetAllPlantsAsync();
                var categories = await _categoryService.GetCategoriesWithAllOptionAsync();
                
                // 更新分类列表
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                SelectedCategory = "全部";

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

        private async void ApplyFilters()
        {
            try
            {
                var filteredPlants = await _plantService.GetFilteredPlantsAsync(SearchText, SelectedCategory);
                
                Plants.Clear();
                foreach (var plant in filteredPlants)
                {
                    Plants.Add(plant);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"筛选植物数据失败: {ex.Message}");
            }
        }
    }
}