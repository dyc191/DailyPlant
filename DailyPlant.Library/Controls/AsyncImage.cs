using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DailyPlant.Library.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DailyPlant.Library.Controls
{
    public class AsyncImage : Image
    {
        public static readonly StyledProperty<string?> SourceUrlProperty =
            AvaloniaProperty.Register<AsyncImage, string?>(nameof(SourceUrl));

        public string? SourceUrl
        {
            get => GetValue(SourceUrlProperty);
            set => SetValue(SourceUrlProperty, value);
        }

        private static Bitmap? _defaultPlaceholder;

        static AsyncImage()
        {
            SourceUrlProperty.Changed.AddClassHandler<AsyncImage>((x, e) => x.OnSourceUrlChanged(e));
            CreateDefaultPlaceholder();
        }

        private static void CreateDefaultPlaceholder()
        {
            try
            {
                // 创建一个简单的灰色占位图
                // 在实际应用中，您可以创建一个更复杂的占位图
                // 这里我们暂时不使用占位图，让背景色显示
                _defaultPlaceholder = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建占位图失败: {ex.Message}");
            }
        }

        private async void OnSourceUrlChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var newUrl = e.NewValue as string;
            
            // 先清空图片
            Source = null;

            if (string.IsNullOrEmpty(newUrl))
            {
                return;
            }

            try
            {
                // 异步加载图片
                var bitmap = await AsyncImageLoader.LoadImageAsync(newUrl);
                
                // 确保在 UI 线程上设置 Source
                if (bitmap != null)
                {
                    Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"异步图片加载失败: {ex.Message}");
            }
        }
    }
}