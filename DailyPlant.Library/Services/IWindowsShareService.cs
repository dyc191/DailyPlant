using System;
using System.Threading.Tasks;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public interface IWindowsShareService
{
    Task ShareToSystemAsync(PlantDetailViewModel plantDetail);
    Task ShareToWeChatAsync(PlantDetailViewModel plantDetail);
    Task ShareToQQAsync(PlantDetailViewModel plantDetail);
}