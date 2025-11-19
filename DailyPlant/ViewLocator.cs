using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DailyPlant.Library.ViewModels;

namespace DailyPlant;

[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var viewModelName = param.GetType().FullName!;
        
        // 将 Library.ViewModels 替换为 Views
        var viewName = viewModelName
            .Replace("Library.ViewModels", "Views")
            .Replace("ViewModel", "View");
        
        // 尝试获取类型
        var type = Type.GetType(viewName);
        
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        // 如果上面失败，尝试在当前程序集中查找
        var currentAssembly = typeof(ViewLocator).Assembly;
        type = currentAssembly.GetType(viewName);
        
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + viewName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}