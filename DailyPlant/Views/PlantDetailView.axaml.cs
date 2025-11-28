using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DailyPlant.Views;

public partial class PlantDetailView : UserControl
{
    public PlantDetailView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.PlantDetailViewModel;
    }
}