using System;
using Avalonia;
using Avalonia.Controls;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;
using DailyPlant.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DailyPlant;

public class ServiceLocator {
    private readonly IServiceProvider _serviceProvider;

    private static ServiceLocator _current;

    public static ServiceLocator Current {
        get
        {
            if (_current is not null) {
                return _current;
            }

            if (Application.Current.TryGetResource(nameof(ServiceLocator),
                    out var resource) &&
                resource is ServiceLocator serviceLocator) {
                return _current = serviceLocator;
            }

            throw new Exception("理论上来讲不应该发生这种情况。");
        }
    }

    public InitializationViewModel InitializationViewModel =>
        _serviceProvider.GetService<InitializationViewModel>();

    public MainWindowViewModel MainWindowViewModel =>
        _serviceProvider.GetService<MainWindowViewModel>();

    public MainViewModel MainViewModel =>
        _serviceProvider.GetService<MainViewModel>();
    
    public TodayPlantViewModel TodayPlantViewModel =>
        _serviceProvider.GetService<TodayPlantViewModel>();
    
    public EncyclopediaViewModel EncyclopediaViewModel =>
        _serviceProvider.GetService<EncyclopediaViewModel>();
    
    public PhotoRecognitionViewModel PhotoRecognitionViewModel =>
        _serviceProvider.GetService<PhotoRecognitionViewModel>();
    
    


    public ServiceLocator() {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<InitializationViewModel>();
        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddSingleton<MainViewModel>();
        
        serviceCollection.AddSingleton<TodayPlantViewModel>();
        serviceCollection.AddSingleton<EncyclopediaViewModel>();
        serviceCollection.AddSingleton<PhotoRecognitionViewModel>();
        
        

        serviceCollection
            .AddSingleton<IRootNavigationService, RootNavigationService>();
        serviceCollection
            .AddSingleton<IMenuNavigationService, MenuNavigationService>();
        
        serviceCollection.AddSingleton<DailyService>();
        
        _serviceProvider = serviceCollection.BuildServiceProvider();
         
    }
}
