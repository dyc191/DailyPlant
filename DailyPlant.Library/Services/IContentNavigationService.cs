using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public interface IContentNavigationService {
    void NavigateTo(string view, object parameter = null);
}

public static class ContentNavigationConstant {
    public const string TodayPlantView = nameof(TodayPlantView);

    public const string PhotoRecognitionView = nameof(PhotoRecognitionView);

    public const string EncyclopediaView = nameof(EncyclopediaView);
}