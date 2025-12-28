using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DailyPlant.Library.Services;

namespace DailyPlant.Views;

public partial class PlantDetailView : UserControl
{
    public PlantDetailView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.PlantDetailViewModel;
        
        // 添加点击外部关闭菜单的事件
        this.AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
    }
    
    private void OnShareOptionsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var popup = this.FindControl<Popup>("ShareMenuPopup");
        if (popup != null)
        {
            popup.IsOpen = !popup.IsOpen;
        }
        e.Handled = true;
    }
    
    private void OnCloseShareMenu(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var popup = this.FindControl<Popup>("ShareMenuPopup");
        if (popup != null)
        {
            popup.IsOpen = false;
        }
    }
    
    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // 如果点击的不是菜单按钮，也不是菜单内部，则关闭菜单
        var shareOptionsButton = this.FindControl<Button>("ShareOptionsButton");
        var shareMenuPopup = this.FindControl<Popup>("ShareMenuPopup");
        
        if (shareOptionsButton != null && shareMenuPopup != null && 
            shareMenuPopup.IsOpen && 
            !shareOptionsButton.IsPointerOver && 
            !shareMenuPopup.IsPointerOver)
        {
            shareMenuPopup.IsOpen = false;
        }
    }
}