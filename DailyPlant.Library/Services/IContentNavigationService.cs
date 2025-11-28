using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public interface IContentNavigationService {
    void NavigateTo(string view, object parameter = null);
}

public static class ContentNavigationConstant {
    public const string TodayPlantView = nameof(TodayPlantView);

    public const string TakePhotoView = nameof(TakePhotoView);

    public const string EncyclopediaView = nameof(EncyclopediaView);
    
    public const string PlantDetailView = nameof(PlantDetailView);
    
    public const string PlantView = nameof(PlantView);
    
}