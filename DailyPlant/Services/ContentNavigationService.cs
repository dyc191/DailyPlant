using System;
using DailyPlant.Library.Services;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Services;

public class ContentNavigationService : IContentNavigationService {
    public void NavigateTo(string view, object parameter = null) {
        ViewModelBase viewModel = view switch {
            ContentNavigationConstant.TodayPlantView => ServiceLocator.Current
                .TodayPlantViewModel,
            ContentNavigationConstant.PhotoRecognitionView => ServiceLocator.Current
                .PhotoRecognitionViewModel,
            ContentNavigationConstant.EncyclopediaView => ServiceLocator.Current
                .EncyclopediaViewModel,
            _ => throw new Exception("未知的视图。")
        };

        if (parameter != null) {
            viewModel.SetParameter(parameter);
        }

        ServiceLocator.Current.MainViewModel.PushContent(viewModel);
    }
}