namespace DailyPlant.Library.Services;

public interface IMenuNavigationService {
    void NavigateTo(string view, object parameter = null);
}

public static class MenuNavigationConstant {
    public const string TodayPlantView = nameof(TodayPlantView);

    public const string EncyclopediaView = nameof(EncyclopediaView);

    public const string PhotoRecognitionView = nameof(PhotoRecognitionView);
}