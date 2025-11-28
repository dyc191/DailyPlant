using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Models;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels
{
    public partial class PlantViewModel : ViewModelBase
    {
        private readonly IContentNavigationService _navigationService;

        [ObservableProperty]
        private Plant _currentPlant;

        [ObservableProperty]
        private bool _isLoading = false;

        public PlantViewModel(IContentNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public override void SetParameter(object parameter)
        {
            if (parameter is Plant plant)
            {
                CurrentPlant = plant;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo(ContentNavigationConstant.EncyclopediaView);
        }
        
    }
}