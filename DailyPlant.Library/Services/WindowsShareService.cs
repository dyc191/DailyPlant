using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DailyPlant.Library.ViewModels;

namespace DailyPlant.Library.Services;

public class WindowsShareService : IWindowsShareService
{
    private readonly IImageTextCombinerService _imageTextCombiner;
    
    public WindowsShareService(IImageTextCombinerService imageTextCombiner)
    {
        _imageTextCombiner = imageTextCombiner;
    }
    
    // 添加一个简单的状态通知事件（可选）
    public event EventHandler<string> ShareStatusChanged;
    
    private void OnShareStatusChanged(string status)
    {
        ShareStatusChanged?.Invoke(this, status);
        Debug.WriteLine($"分享状态: {status}");
    }
    
    public async Task ShareToSystemAsync(PlantDetailViewModel plantDetail)
    {
        try
        {
            OnShareStatusChanged("开始系统分享...");
            
            var shareContent = BuildShareContent(plantDetail);
            
            string combinedImagePath = null;
            if (plantDetail.PlantImage != null)
            {
                OnShareStatusChanged("正在创建合成图片...");
                combinedImagePath = await _imageTextCombiner.CreateCombinedImageAsync(shareContent, plantDetail.PlantImage);
            }
            
            if (!string.IsNullOrEmpty(combinedImagePath) && File.Exists(combinedImagePath))
            {
                OnShareStatusChanged("正在打开图片...");
                
                // 使用系统默认程序打开图片
                Process.Start(new ProcessStartInfo
                {
                    FileName = combinedImagePath,
                    UseShellExecute = true
                });
                
                OnShareStatusChanged("图片已打开，请使用系统分享功能");
                
                // 延迟清理临时文件（60秒后）
                _ = Task.Delay(60000).ContinueWith(_ =>
                {
                    try
                    {
                        if (File.Exists(combinedImagePath))
                        {
                            File.Delete(combinedImagePath);
                            Debug.WriteLine("临时文件已清理");
                        }
                    }
                    catch { }
                });
            }
            else
            {
                OnShareStatusChanged("正在复制文本...");
                await CopyTextToClipboardAsync(shareContent);
            }
            
            OnShareStatusChanged("系统分享完成");
        }
        catch (Exception ex)
        {
            OnShareStatusChanged($"分享失败: {ex.Message}");
            Debug.WriteLine($"系统分享异常: {ex}");
        }
    }
    
    public async Task ShareToWeChatAsync(PlantDetailViewModel plantDetail)
    {
        await ShareToSocialAppAsync(plantDetail, "微信", TryOpenWeChat);
    }
    
    public async Task ShareToQQAsync(PlantDetailViewModel plantDetail)
    {
        await ShareToSocialAppAsync(plantDetail, "QQ", TryOpenQQ);
    }
    
    private async Task ShareToSocialAppAsync(PlantDetailViewModel plantDetail, string appName, Action openAppAction)
    {
        try
        {
            OnShareStatusChanged($"开始分享到{appName}...");
            
            var shareContent = BuildShareContent(plantDetail);
            
            string combinedImagePath = null;
            if (plantDetail.PlantImage != null)
            {
                OnShareStatusChanged("正在创建合成图片...");
                combinedImagePath = await _imageTextCombiner.CreateCombinedImageAsync(shareContent, plantDetail.PlantImage);
            }
            
            if (!string.IsNullOrEmpty(combinedImagePath) && File.Exists(combinedImagePath))
            {
                OnShareStatusChanged("正在复制图片到剪贴板...");
                
                // 尝试多种方法复制图片到剪贴板
                bool success = false;
                
                // 方法1: 使用改进的PowerShell方法
                success = await CopyImageToClipboardWithPowerShellAsync(combinedImagePath);
                
                if (!success)
                {
                    // 方法2: 备用PowerShell方法
                    OnShareStatusChanged("方法1失败，尝试备用方法...");
                    success = await CopyImageToClipboardWithPowerShellAltAsync(combinedImagePath);
                }
                
                if (success)
                {
                    OnShareStatusChanged($"图片已复制到剪贴板，正在启动{appName}...");
                    
                    // 打开目标应用
                    openAppAction?.Invoke();
                    
                    OnShareStatusChanged($"{appName}已启动，请粘贴发送！");
                    
                    // 延迟清理临时文件（30秒后）
                    _ = Task.Delay(30000).ContinueWith(_ =>
                    {
                        try
                        {
                            if (File.Exists(combinedImagePath))
                            {
                                File.Delete(combinedImagePath);
                                Debug.WriteLine("临时文件已清理");
                            }
                        }
                        catch { }
                    });
                }
                else
                {
                    OnShareStatusChanged($"复制失败，正在打开图片...");
                    // 如果复制失败，打开图片让用户手动操作
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = combinedImagePath,
                        UseShellExecute = true
                    });
                    
                    OnShareStatusChanged($"图片已打开，请手动分享到{appName}");
                }
            }
            else
            {
                OnShareStatusChanged("正在复制文本...");
                await CopyTextToClipboardAsync(shareContent);
            }
            
            OnShareStatusChanged($"{appName}分享完成");
        }
        catch (Exception ex)
        {
            OnShareStatusChanged($"{appName}分享失败: {ex.Message}");
            Debug.WriteLine($"{appName}分享异常: {ex}");
        }
    }
    
    // 方法1: 使用外部PowerShell脚本文件
    private async Task<bool> CopyImageToClipboardWithPowerShellAsync(string imagePath)
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                Debug.WriteLine("图片文件不存在");
                return false;
            }
            
            // 创建外部PowerShell脚本
            var scriptContent = @"
param($ImagePath)

try {
    Add-Type -AssemblyName System.Drawing
    Add-Type -AssemblyName System.Windows.Forms
    
    $image = [System.Drawing.Image]::FromFile($ImagePath)
    [System.Windows.Forms.Clipboard]::Clear()
    [System.Windows.Forms.Clipboard]::SetImage($image)
    $image.Dispose()
    
    return $true
}
catch {
    Write-Error $_.Exception.Message
    return $false
}
";
            
            var tempScriptPath = Path.Combine(Path.GetTempPath(), $"clipboard_script_{Guid.NewGuid()}.ps1");
            await File.WriteAllTextAsync(tempScriptPath, scriptContent);
            
            // 使用 -Command 参数直接执行PowerShell代码
            var escapedImagePath = imagePath.Replace("'", "''");
            var command = $"-NoProfile -ExecutionPolicy Bypass -Command \". '{tempScriptPath}' -ImagePath '{escapedImagePath}'\"";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            var completed = process.WaitForExit(10000);
            
            if (!completed)
            {
                process.Kill();
                Debug.WriteLine("PowerShell 执行超时");
            }
            
            // 清理脚本文件
            try { File.Delete(tempScriptPath); } catch { }
            
            if (process.ExitCode == 0 && output.Trim() == "True")
            {
                Debug.WriteLine("PowerShell 复制图片到剪贴板成功");
                return true;
            }
            
            if (!string.IsNullOrEmpty(error))
                Debug.WriteLine($"PowerShell 错误: {error}");
            
            Debug.WriteLine($"PowerShell 复制失败，退出码: {process.ExitCode}, 输出: {output}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PowerShell 复制失败: {ex.Message}");
            return false;
        }
    }
    
    // 方法2: 备用方法 - 使用内联PowerShell命令
    private async Task<bool> CopyImageToClipboardWithPowerShellAltAsync(string imagePath)
    {
        try
        {
            // 使用内联命令，避免文件权限问题
            var escapedPath = imagePath.Replace("'", "''").Replace("\"", "\"\"");
            var command = $@"
-NoProfile -ExecutionPolicy Bypass -Command ""
try {{
    Add-Type -AssemblyName System.Drawing
    Add-Type -AssemblyName System.Windows.Forms
    `$img = [System.Drawing.Image]::FromFile('{escapedPath}')
    [System.Windows.Forms.Clipboard]::Clear()
    [System.Windows.Forms.Clipboard]::SetImage(`$img)
    `$img.Dispose()
    Write-Output 'SUCCESS'
}}
catch {{
    Write-Error `$_.Exception.Message
    exit 1
}}
""
";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            process.WaitForExit(10000);
            
            if (process.ExitCode == 0 && output.Contains("SUCCESS"))
            {
                Debug.WriteLine("备用PowerShell方法成功");
                return true;
            }
            
            Debug.WriteLine($"备用方法失败: 退出码={process.ExitCode}, 输出={output}, 错误={error}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"备用PowerShell方法异常: {ex.Message}");
            return false;
        }
    }
    
    // 使用 Avalonia 剪贴板复制文本
    private async Task<bool> CopyTextToClipboardAsync(string text)
    {
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(text);
                Debug.WriteLine("文本已复制到剪贴板");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"复制文本到剪贴板失败: {ex.Message}");
            return false;
        }
    }
    
    private void TryOpenWeChat()
    {
        try
        {
            // 先尝试通过协议启动
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "weixin:",
                    UseShellExecute = true
                });
                Debug.WriteLine("通过协议启动微信");
                return;
            }
            catch { }
            
            // 如果协议失败，尝试常见安装路径
            var wechatPaths = new[]
            {
                @"C:\Program Files (x86)\Tencent\WeChat\WeChat.exe",
                @"C:\Program Files\Tencent\WeChat\WeChat.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Tencent", "WeChat", "WeChat.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tencent", "WeChat", "WeChat.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tencent", "WeChat", "WeChat.exe"),
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Tencent\WeChat\WeChat.exe"
            };
            
            foreach (var path in wechatPaths)
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    Debug.WriteLine($"通过路径启动微信: {path}");
                    return;
                }
            }
            
            Debug.WriteLine("未找到微信安装路径");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"启动微信失败: {ex.Message}");
        }
    }
    
    private void TryOpenQQ()
    {
        try
        {
            // 先尝试通过协议启动
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "tencent:",
                    UseShellExecute = true
                });
                Debug.WriteLine("通过协议启动QQ");
                return;
            }
            catch { }
            
            // 如果协议失败，尝试常见安装路径
            var qqPaths = new[]
            {
                @"C:\Program Files (x86)\Tencent\QQ\Bin\QQ.exe",
                @"C:\Program Files\Tencent\QQ\Bin\QQ.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Tencent", "QQ", "Bin", "QQ.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tencent", "QQ", "Bin", "QQ.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tencent", "QQ", "Bin", "QQ.exe"),
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Tencent\QQ\Bin\QQ.exe"
            };
            
            foreach (var path in qqPaths)
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    Debug.WriteLine($"通过路径启动QQ: {path}");
                    return;
                }
            }
            
            Debug.WriteLine("未找到QQ安装路径");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"启动QQ失败: {ex.Message}");
        }
    }
    
    private string BuildShareContent(PlantDetailViewModel plantDetail)
    {
        var builder = new StringBuilder();
        
        builder.AppendLine("🌿 植物识别结果 🌿");
        builder.AppendLine();
        builder.AppendLine($"🔍 植物名称: {plantDetail.PlantName}");
        builder.AppendLine($"📊 识别可信度: {plantDetail.Score * 100:F1}%");
        builder.AppendLine();
        
        if (!string.IsNullOrEmpty(plantDetail.Description))
        {
            var cleanDesc = plantDetail.Description
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();
            
            if (cleanDesc.Length > 500)
                cleanDesc = cleanDesc.Substring(0, 500) + "...";
            
            builder.AppendLine($"📝 描述: {cleanDesc}");
            builder.AppendLine();
        }
        
        builder.AppendLine("✨ 来自「每日一植」应用");
        
        return builder.ToString();
    }
    
    private TopLevel GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return TopLevel.GetTopLevel(desktop.MainWindow);
        }
        
        return null;
    }
}