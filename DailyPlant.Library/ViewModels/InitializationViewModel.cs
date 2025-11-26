using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels;

public class InitializationViewModel : ViewModelBase {
    private readonly IRootNavigationService _rootNavigationService;
    private readonly IMenuNavigationService _menuNavigationService;

    public InitializationViewModel(
        IRootNavigationService rootNavigationService,
        IMenuNavigationService menuNavigationService
        ) {
      
        _rootNavigationService = rootNavigationService;
        _menuNavigationService = menuNavigationService;
      

        OnInitializedCommand = new AsyncRelayCommand(OnInitializedAsync);
    }

    private ICommand OnInitializedCommand { get; }

    public async Task OnInitializedAsync() {

        await Task.Delay(1000);

        _rootNavigationService.NavigateTo(RootNavigationConstant.MainView);
        _menuNavigationService.NavigateTo(MenuNavigationConstant.TodayPlantView);
    }
}