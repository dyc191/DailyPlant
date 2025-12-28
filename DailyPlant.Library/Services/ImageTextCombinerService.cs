using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace DailyPlant.Library.Services;

public class ImageTextCombinerService : IImageTextCombinerService
{
    public async Task<string> CreateCombinedImageAsync(string textContent, Bitmap originalImage)
    {
        try
        {
            Debug.WriteLine("开始创建合成图片...");
            
            // 创建临时目录
            var tempDir = Path.Combine(Path.GetTempPath(), "DailyPlant_Combined");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            
            // 生成输出路径
            var outputPath = Path.Combine(tempDir, $"plant_combined_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            
            // 将 Avalonia Bitmap 转换为 SKBitmap
            var skBitmap = await ConvertToSkBitmapAsync(originalImage);
            if (skBitmap == null)
            {
                Debug.WriteLine("图片转换失败");
                return null;
            }
            
            Debug.WriteLine($"原始图片尺寸: {skBitmap.Width}x{skBitmap.Height}");
            
            // 计算合成图片尺寸
            int maxWidth = 800;
            int imageWidth = Math.Min(skBitmap.Width, maxWidth);
            int imageHeight = (int)(skBitmap.Height * ((float)imageWidth / skBitmap.Width)); // 按比例缩放高度
            
            // 重新缩放图片
            var resizedImageInfo = new SKImageInfo(imageWidth, imageHeight);
            using var resizedBitmap = skBitmap.Resize(resizedImageInfo, SKFilterQuality.High);
            
            // 估计文本高度
            int estimatedTextHeight = EstimateTextHeight(textContent, imageWidth - 60);
            int textAreaHeight = Math.Min(estimatedTextHeight + 100, 600); // 最大600像素
            int totalHeight = imageHeight + textAreaHeight;
            
            Debug.WriteLine($"合成图片尺寸: {imageWidth}x{totalHeight}");
            
            // 创建新的 SKSurface
            using var surface = SKSurface.Create(new SKImageInfo(imageWidth, totalHeight));
            var canvas = surface.Canvas;
            
            // 1. 绘制背景
            canvas.Clear(SKColors.White);
            
            // 2. 绘制原始图片
            canvas.DrawBitmap(resizedBitmap, 0, 0);
            
            // 3. 绘制文字区域背景
            using var textBackground = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, imageHeight, imageWidth, textAreaHeight, textBackground);
            
            // 4. 绘制分隔线
            using var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#4CAF50"),
                StrokeWidth = 3,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };
            canvas.DrawLine(30, imageHeight, imageWidth - 30, imageHeight, linePaint);
            
            // 5. 绘制标题
            using var titlePaint = new SKPaint
            {
                Color = SKColor.Parse("#2E7D32"),
                TextSize = 28,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Microsoft YaHei", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                TextAlign = SKTextAlign.Center
            };
            
            // 测量标题宽度并绘制
            float titleY = imageHeight + 50;
            canvas.DrawText("🌿 植物识别结果 🌿", imageWidth / 2, titleY, titlePaint);
            
            // 6. 绘制主要内容
            float contentY = titleY + 40;
            float contentWidth = imageWidth - 40;
            
            // 植物名称（从文本内容中提取）
            var plantName = ExtractPlantName(textContent);
            var nameLines = WrapText($"🔍 植物名称: {plantName}", 
                contentWidth, 20, SKFontStyleWeight.Bold);
            
            using var namePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 20,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Microsoft YaHei", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };
            
            foreach (var line in nameLines)
            {
                if (contentY + 30 > totalHeight - 50) break;
                canvas.DrawText(line, 20, contentY, namePaint);
                contentY += 28;
            }
            
            contentY += 10;
            
            // 识别可信度
            var scoreText = ExtractConfidenceScore(textContent);
            var scoreLines = WrapText($"📊 {scoreText}", contentWidth, 18);
            
            using var scorePaint = new SKPaint
            {
                Color = SKColor.Parse("#2196F3"),
                TextSize = 18,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Microsoft YaHei")
            };
            
            foreach (var line in scoreLines)
            {
                if (contentY + 26 > totalHeight - 50) break;
                canvas.DrawText(line, 20, contentY, scorePaint);
                contentY += 26;
            }
            
            contentY += 10;
            
            // 植物描述
            var descriptionText = ExtractDescription(textContent);
            var descLines = WrapText($"📝 {descriptionText}", contentWidth, 16);
            
            using var descPaint = new SKPaint
            {
                Color = SKColors.DarkGray,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Microsoft YaHei")
            };
            
            foreach (var line in descLines)
            {
                if (contentY + 24 > totalHeight - 50) break;
                canvas.DrawText(line, 20, contentY, descPaint);
                contentY += 22;
            }
            
            // 7. 绘制底部信息
            using var footerPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 14,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Microsoft YaHei"),
                TextAlign = SKTextAlign.Center
            };
            
            canvas.DrawText("✨ 来自「每日一植」应用", imageWidth / 2, totalHeight - 20, footerPaint);
            
            // 8. 保存图片
            using var image = surface.Snapshot();
            
            // 使用 PNG 格式编码，质量为 90
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            
            await using (var stream = File.Create(outputPath))
            {
                data.SaveTo(stream);
            }
            
            // 释放资源
            skBitmap.Dispose();
            
            Debug.WriteLine($"合成图片创建完成: {outputPath}");
            if (File.Exists(outputPath))
            {
                var fileInfo = new FileInfo(outputPath);
                Debug.WriteLine($"文件大小: {fileInfo.Length / 1024}KB");
            }
            
            return outputPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"创建合成图片失败: {ex.Message}");
            Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            return null;
        }
    }
    
    private async Task<SKBitmap> ConvertToSkBitmapAsync(Bitmap avaloniaBitmap)
    {
        try
        {
            if (avaloniaBitmap == null)
                return null;
            
            await using var memoryStream = new MemoryStream();
            
            // 将 Avalonia Bitmap 保存到内存流
            avaloniaBitmap.Save(memoryStream);
            memoryStream.Position = 0;
            
            // 使用 SkiaSharp 加载图片
            return SKBitmap.Decode(memoryStream);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"转换图片格式失败: {ex.Message}");
            return null;
        }
    }
    
    private int EstimateTextHeight(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return 150;
        
        // 简单估算：每50个字符一行，每行30像素
        int approxLineCount = text.Length / 50 + 1;
        return approxLineCount * 30 + 80; // 加上标题和边距
    }
    
    private List<string> WrapText(string text, float maxWidth, int fontSize, SKFontStyleWeight fontWeight = SKFontStyleWeight.Normal)
    {
        var lines = new List<string>();
        
        if (string.IsNullOrEmpty(text))
            return lines;
        
        using var typeface = SKTypeface.FromFamilyName("Microsoft YaHei", fontWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var paint = new SKPaint
        {
            TextSize = fontSize,
            Typeface = typeface
        };
        
        // 先按换行符分割
        var paragraphs = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var paragraph in paragraphs)
        {
            var words = paragraph.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();
            
            foreach (var word in words)
            {
                var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                var width = paint.MeasureText(testLine);
                
                if (width <= maxWidth)
                {
                    if (currentLine.Length > 0)
                        currentLine.Append(" ");
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                    }
                    
                    // 如果单个词就超过最大宽度，需要强制换行
                    var wordWidth = paint.MeasureText(word);
                    if (wordWidth > maxWidth)
                    {
                        // 将长词分割成字符
                        var chars = word.ToCharArray();
                        var currentWord = new StringBuilder();
                        
                        foreach (var ch in chars)
                        {
                            var testChar = currentWord.ToString() + ch;
                            if (paint.MeasureText(testChar) > maxWidth)
                            {
                                lines.Add(currentWord.ToString());
                                currentWord.Clear();
                            }
                            currentWord.Append(ch);
                        }
                        
                        if (currentWord.Length > 0)
                        {
                            currentLine.Append(currentWord.ToString());
                        }
                    }
                    else
                    {
                        currentLine.Append(word);
                    }
                }
            }
            
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
            }
        }
        
        return lines;
    }
    
    private string ExtractPlantName(string fullText)
    {
        var lines = fullText.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("🔍 植物名称:"))
            {
                return line.Replace("🔍 植物名称:", "").Trim();
            }
        }
        
        return "未知植物";
    }
    
    private string ExtractConfidenceScore(string fullText)
    {
        var lines = fullText.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("📊 识别可信度:"))
            {
                return line.Replace("📊 识别可信度:", "").Trim();
            }
            else if (line.Contains("识别可信度:"))
            {
                return line.Replace("识别可信度:", "").Trim();
            }
        }
        
        return "未知可信度";
    }
    
    private string ExtractDescription(string fullText)
    {
        var lines = fullText.Split('\n');
        bool foundDescription = false;
        var descriptionBuilder = new StringBuilder();
        
        foreach (var line in lines)
        {
            if (line.Contains("📝 描述:"))
            {
                descriptionBuilder.Append(line.Replace("📝 描述:", "").Trim());
                foundDescription = true;
            }
            else if (foundDescription && !string.IsNullOrWhiteSpace(line) && 
                     !line.Contains("✨ 来自「每日一植」应用"))
            {
                descriptionBuilder.Append(" " + line.Trim());
            }
        }
        
        var desc = descriptionBuilder.ToString().Trim();
        if (string.IsNullOrEmpty(desc))
        {
            return "该植物暂无详细描述信息";
        }
        
        // 限制描述长度
        return desc.Length > 300 ? desc.Substring(0, 300) + "..." : desc;
    }
}