using Avalonia.Input;
using DailyPlant.Library.Models;
using DailyPlant.Library.Services;
using DailyPlant.Library.Commands;
using DailyPlant.Library.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using DailyPlant.Library.Helpers;

namespace DailyPlant.Library.ViewModels
{
    public class TodayPlantViewModel : ViewModelBase
    {
        private readonly DailyService _plantService;
        private Plant _currentPlant;
        private bool _isLoading;
        private Bitmap? _plantImage;

        public TodayPlantViewModel(DailyService plantService)
        {
            _plantService = plantService;
            LoadSeasonalPlantCommand = new AsyncRelayCommand(LoadSeasonalPlantAsync);
            
            // 初始化加载一个当季植物
            _ = LoadSeasonalPlantAsync();
        }

        public Plant CurrentPlant
        {
            get => _currentPlant;
            set
            {
                _currentPlant = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDetails));
                _ = LoadPlantImageAsync();
            }
        }
        
        public Bitmap? PlantImage
        {
            get => _plantImage;
            set
            {
                _plantImage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        public bool HasDetails
        {
            get
            {
                if (CurrentPlant == null) return false;
                
                return !string.IsNullOrEmpty(CurrentPlant.Category) ||
                       !string.IsNullOrEmpty(CurrentPlant.SeasonSowing) ||
                       !string.IsNullOrEmpty(CurrentPlant.SeasonBloom) ||
                       !string.IsNullOrEmpty(CurrentPlant.Zone) ||
                       !string.IsNullOrEmpty(CurrentPlant.Water);
            }
        }

        public ICommand LoadSeasonalPlantCommand { get; }

        private async Task LoadSeasonalPlantAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                string currentSeason = GetCurrentSeason();
                CurrentPlant = await _plantService.GetPlantBySeasonAsync(currentSeason);
            }
            catch (System.Exception ex)
            {
                // 处理错误
                System.Diagnostics.Debug.WriteLine($"加载当季植物数据失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private string GetCurrentSeason()
        {
            int month = DateTime.Now.Month;
            
            // 春季: 3-5月
            if (month >= 3 && month <= 5)
                return "春";
            // 夏季: 6-8月
            else if (month >= 6 && month <= 8)
                return "夏";
            // 秋季: 9-11月
            else if (month >= 9 && month <= 11)
                return "秋";
            // 冬季: 12-2月
            else
                return "冬";
        }
        
        private async Task LoadPlantImageAsync()
        {
            if (CurrentPlant == null || string.IsNullOrEmpty(CurrentPlant.Image))
            {
                PlantImage = null;
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"开始加载植物图片: {CurrentPlant.Image}");
                PlantImage = await ImageLoader.LoadImageAsync(CurrentPlant.Image);
                System.Diagnostics.Debug.WriteLine($"图片加载完成: {PlantImage != null}");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载植物图片失败: {ex.Message}");
                PlantImage = null;
            }
        }
        
    }
}



