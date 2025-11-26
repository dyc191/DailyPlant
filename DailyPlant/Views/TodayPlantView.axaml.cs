using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Views
{
    public partial class TodayPlantView : UserControl
    {
        public TodayPlantView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
