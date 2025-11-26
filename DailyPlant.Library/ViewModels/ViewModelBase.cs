using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyPlant.Library.ViewModels;

public abstract class ViewModelBase : ObservableObject {
    public virtual void SetParameter(object parameter) { }
}