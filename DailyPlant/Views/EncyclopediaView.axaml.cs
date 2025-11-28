using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DailyPlant.Views
{
    public partial class EncyclopediaView : UserControl
    {
        public EncyclopediaView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}