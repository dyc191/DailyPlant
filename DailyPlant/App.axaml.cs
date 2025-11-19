using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;

using DailyPlant.Views;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace DailyPlant;

public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            desktop) {
            desktop.MainWindow = new MainWindow();

        }

        base.OnFrameworkInitializationCompleted();
    }
}