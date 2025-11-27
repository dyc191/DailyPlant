using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.Input;
using DailyPlant.Library.Services;

namespace DailyPlant.Library.ViewModels;

public class MainViewModel : ViewModelBase {
    private readonly IMenuNavigationService _menuNavigationService;
    

    public MainViewModel(IMenuNavigationService menuNavigationService)
    {
        _menuNavigationService = menuNavigationService;

        OpenPaneCommand = new RelayCommand(OpenPane);
        ClosePaneCommand = new RelayCommand(ClosePane);
        GoBackCommand = new RelayCommand(GoBack);
        OnMenuTappedCommand = new RelayCommand<object>(OnMenuTapped); 
    
        // 设置初始选中的菜单项
        SelectedMenuItem = MenuItem.MenuItems.First();
    }
    
    public void OnMenuTapped(object parameter)
    {
        if (parameter is string viewName)
        {
            // 根据视图名称找到对应的菜单项
            var menuItem = MenuItem.MenuItems.FirstOrDefault(p => p.View == viewName);
            if (menuItem != null)
            {
                SelectedMenuItem = menuItem;
                _menuNavigationService.NavigateTo(menuItem.View);
            }
        }
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

public class MenuItem
{
    public string View { get; set; }
    public string Name { get; set; }

    public MenuItem() { }

    private MenuItem(string name, string view) 
    { 
        Name = name; 
        View = view; 
    }

    private static MenuItem TodayPlantView =>
        new("今日植物", MenuNavigationConstant.TodayPlantView);

    private static MenuItem EncyclopediaView =>
        new("植物百科", MenuNavigationConstant.EncyclopediaView);

    private static MenuItem PhotoRecognitionView =>
        new("拍照识图", MenuNavigationConstant.PhotoRecognitionView);

    public static IEnumerable<MenuItem> MenuItems { get; } = [
        TodayPlantView, EncyclopediaView, PhotoRecognitionView
    ];
}

public class MenuItemEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MenuItem selectedItem && parameter is MenuItem currentItem)
        {
            return selectedItem.View == currentItem.View;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
