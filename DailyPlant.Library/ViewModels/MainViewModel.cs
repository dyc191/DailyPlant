using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels;

public class MainViewModel : ViewModelBase {
    private readonly IMenuNavigationService _menuNavigationService;

    public MainViewModel(IMenuNavigationService menuNavigationService) {
        _menuNavigationService = menuNavigationService;

        OpenPaneCommand = new RelayCommand(OpenPane);
        ClosePaneCommand = new RelayCommand(ClosePane);
        GoBackCommand = new RelayCommand(GoBack);
        OnMenuTappedCommand = new RelayCommand(OnMenuTapped);
    }

    private string _title = "DailyPlant";

    public string Title {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    private bool _isPaneOpen;

    public bool IsPaneOpen {
        get => _isPaneOpen;
        private set => SetProperty(ref _isPaneOpen, value);
    }

    private ViewModelBase _content;

    public ViewModelBase Content {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public ICommand OpenPaneCommand { get; }

    public void OpenPane() => IsPaneOpen = true;

    public ICommand ClosePaneCommand { get; }

    public void ClosePane() => IsPaneOpen = false;

    public void PushContent(ViewModelBase content) =>
        ContentStack.Insert(0, Content = content);

    public void SetMenuAndContent(string view, ViewModelBase content) {
        ContentStack.Clear();
        PushContent(content);
        SelectedMenuItem =
            MenuItem.MenuItems.FirstOrDefault(p => p.View == view);
        Title = SelectedMenuItem.Name;
        IsPaneOpen = false;
    }

    private MenuItem _selectedMenuItem;

    public MenuItem SelectedMenuItem {
        get => _selectedMenuItem;
        set => SetProperty(ref _selectedMenuItem, value);
    }

    public ICommand OnMenuTappedCommand { get; }

    public void OnMenuTapped() {
        if (SelectedMenuItem is null) {
            return;
        }

        _menuNavigationService.NavigateTo(SelectedMenuItem.View);
    }

    public ObservableCollection<ViewModelBase> ContentStack { get; } = [];

    public ICommand GoBackCommand { get; }

    public void GoBack() {
        if (ContentStack.Count <= 1) {
            return;
        }

        ContentStack.RemoveAt(0);
        Content = ContentStack[0];
    }
}

public class MenuItem {
    public string View { get; private init; }
    public string Name { get; private init; }

    private MenuItem() { }

    private static MenuItem TodayPlantView =>
        new() { Name = "今日植物", View = MenuNavigationConstant.TodayPlantView };

    private static MenuItem EncyclopediaView =>
        new() { Name = "植物百科", View = MenuNavigationConstant.EncyclopediaView };

    private static MenuItem TakePhotoView =>
        new() { Name = "拍照识图", View = MenuNavigationConstant.TakePhotoView };

    public static IEnumerable<MenuItem> MenuItems { get; } = [
        TodayPlantView, EncyclopediaView,TakePhotoView
    ];
}