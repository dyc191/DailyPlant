using System;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;
using ViewModelBase = DailyPlant.Library.ViewModels.ViewModelBase;

namespace DailyPlant.Services;

public class MenuNavigationService : IMenuNavigationService {
    public void NavigateTo(string view, object parameter = null) {
        ViewModelBase viewModel = view switch {
            MenuNavigationConstant.TodayPlantView => ServiceLocator.Current
                .TodayPlantViewModel,
            MenuNavigationConstant.EncyclopediaView => ServiceLocator.Current
                .EncyclopediaViewModel,
            MenuNavigationConstant.PhotoRecognitionView => ServiceLocator.Current
                .PhotoRecognitionViewModel,
            _ => throw new Exception("未知的视图。")
        };

        if (parameter is not null) {
            viewModel.SetParameter(parameter);
        }

        ServiceLocator.Current.MainViewModel.SetMenuAndContent(view, viewModel);
    }
}