using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DailyPlant.Views;

public partial class TakePhotoView : UserControl
{
    public TakePhotoView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.TakePhotoViewModel;
    }
}