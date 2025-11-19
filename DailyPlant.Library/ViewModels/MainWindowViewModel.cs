using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels;

public class MainWindowViewModel : ViewModelBase {
  
    private readonly IRootNavigationService _rootNavigationService;
    private readonly IMenuNavigationService _menuNavigationService;

    public MainWindowViewModel(
        IRootNavigationService rootNavigationService,
        IMenuNavigationService menuNavigationService
    ) {
       
        _rootNavigationService = rootNavigationService;
        _menuNavigationService = menuNavigationService;
    

        OnInitializedCommand = new RelayCommand(OnInitialized);
    }

    private ViewModelBase _content;

    public ViewModelBase Content {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public ICommand OnInitializedCommand { get; }

    public void OnInitialized()
    {
            _rootNavigationService.NavigateTo(RootNavigationConstant.MainView);
            _menuNavigationService.NavigateTo(MenuNavigationConstant.TodayPlantView);
    }
}