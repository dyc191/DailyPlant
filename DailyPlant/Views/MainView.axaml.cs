using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace DailyPlant.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 初始高亮
            UpdateButtonHighlights();
            
            // 监听数据上下文变化
            if (DataContext is Library.ViewModels.MainViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(Library.ViewModels.MainViewModel.SelectedMenuItem))
                    {
                        UpdateButtonHighlights();
                    }
                };
            }
        }

        private void UpdateButtonHighlights()
        {
            if (DataContext is not Library.ViewModels.MainViewModel viewModel)
                return;

            // 重置所有按钮样式
            ResetButtonStyle(TodayPlantButton);
            ResetButtonStyle(EncyclopediaButton);
            ResetButtonStyle(PhotoRecognitionButton);

            // 高亮当前选中的按钮
            var selectedView = viewModel.SelectedMenuItem?.View;
            if (selectedView == Library.Services.MenuNavigationConstant.TodayPlantView)
            {
                ApplySelectedStyle(TodayPlantButton);
            }
            else if (selectedView == Library.Services.MenuNavigationConstant.EncyclopediaView)
            {
                ApplySelectedStyle(EncyclopediaButton);
            }
            else if (selectedView == Library.Services.MenuNavigationConstant.TakePhotoView)
            {
                ApplySelectedStyle(PhotoRecognitionButton);
            }
        }

        private void ResetButtonStyle(Button button)
        {
            button.Background = Brushes.Transparent;
            button.Foreground = new SolidColorBrush(Color.Parse("#1B5E20"));
            button.BorderBrush = Brushes.Transparent;
        }

        private void ApplySelectedStyle(Button button)
        {
            button.Background = new SolidColorBrush(Color.Parse("#2E7D32"));
            button.Foreground = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.Parse("#1B5E20"));
        }
    }
}

