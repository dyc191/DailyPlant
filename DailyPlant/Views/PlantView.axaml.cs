using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DailyPlant.Views
{
    public partial class PlantView : UserControl
    {
        public PlantView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}