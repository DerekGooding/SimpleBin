using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SimpleBin;

public partial class OptimizedSimpleBin : ApplicationContext
{
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;
    private System.Windows.Forms.Timer? _debounceTimer;
    private CancellationTokenSource? _cancellationTokenSource;

    // Cached icons to avoid recreation
    private static Icon? _emptyIcon;
    private static Icon? _fullIcon;
    private static Icon? _emptyIconDark;
    private static Icon? _fullIconDark;

    // Recycle bin monitoring
    private FileSystemWatcher? _recycleBinWatcher;
    private bool _isRecycleBinEmpty = true;
    private readonly Lock _lockObject = new();

    // Performance optimization flags
    private bool _isUpdating = false;
    private DateTime _lastUpdateTime = DateTime.MinValue;
    private const int DEBOUNCE_INTERVAL = 500; // ms
    private const int MIN_UPDATE_INTERVAL = 250; // ms

    public OptimizedSimpleBin()
    {
        InitializeComponent();
        SetupRecycleBinMonitoring();
        _ = InitializeAsync();
    }

    private void InitializeComponent()
    {
        // Create context menu
        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("View Recycle Bin Info", null, OnViewInfo);
        _contextMenu.Items.Add("Empty Recycle Bin", null, OnEmptyRecycleBin);
        _contextMenu.Items.Add("-"); // Separator
        _contextMenu.Items.Add("Settings", null, OnSettings);
        _contextMenu.Items.Add("Exit", null, OnExit);

        // Create notify icon
        _notifyIcon = new NotifyIcon
        {
            Icon = GetCachedIcon(true, IsSystemDarkTheme()),
            Text = "SimpleBin - Recycle Bin Manager",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        _notifyIcon.Click += OnNotifyIconClick;

        // Setup debounce timer
        _debounceTimer = new System.Windows.Forms.Timer
        {
            Interval = DEBOUNCE_INTERVAL
        };
        _debounceTimer.Tick += OnDebounceTimerTick;

        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async Task InitializeAsync()
    {
        // Initial status check in background
        await UpdateRecycleBinStatusAsync();

        // Trim working set after initialization
        TrimWorkingSet();
    }

    private void SetupRecycleBinMonitoring()
    {
        try
        {
            // Get recycle bin path for current user
            var recycleBinPath = GetRecycleBinPath();

            if (Directory.Exists(recycleBinPath))
            {
                _recycleBinWatcher = new FileSystemWatcher(recycleBinPath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                _recycleBinWatcher.Created += OnRecycleBinChanged;
                _recycleBinWatcher.Deleted += OnRecycleBinChanged;
                _recycleBinWatcher.Changed += OnRecycleBinChanged;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Failed to setup RecycleBin monitoring: {ex.Message}");
        }
    }

    private void OnRecycleBinChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid changes
        if (_isUpdating) return;

        var now = DateTime.Now;
        if ((now - _lastUpdateTime).TotalMilliseconds < MIN_UPDATE_INTERVAL)
        {
            // Restart debounce timer
            _debounceTimer?.Stop();
            _debounceTimer?.Start();
            return;
        }

        _lastUpdateTime = now;
        _debounceTimer?.Stop();
        _debounceTimer?.Start();
    }

    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        _debounceTimer?.Stop();
        _ = UpdateRecycleBinStatusAsync();
    }

    private async Task UpdateRecycleBinStatusAsync()
    {
        if (_isUpdating) return;

        lock (_lockObject)
        {
            if (_isUpdating) return;
            _isUpdating = true;
        }

        try
        {
            // Check recycle bin status in background thread
            var isEmpty = await Task.Run(IsRecycleBinEmpty, _cancellationTokenSource?.Token ?? CancellationToken.None);

            if (isEmpty != _isRecycleBinEmpty)
            {
                _isRecycleBinEmpty = isEmpty;

                // Update UI on main thread
                if (_notifyIcon?.Icon != null)
                {
                    var oldIcon = _notifyIcon.Icon;
                    _notifyIcon.Icon = GetCachedIcon(isEmpty, IsSystemDarkTheme());

                    // Don't dispose cached icons
                    if (oldIcon != _emptyIcon && oldIcon != _fullIcon &&
                        oldIcon != _emptyIconDark && oldIcon != _fullIconDark)
                    {
                        oldIcon.Dispose();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating recycle bin status: {ex.Message}");
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private Icon GetCachedIcon(bool isEmpty, bool isDarkTheme) => isDarkTheme
            ? isEmpty ? (_emptyIcon ??= LoadEmbeddedIcon("empty.ico")) : (_fullIcon ??= LoadEmbeddedIcon("full.ico"))
            : isEmpty ? (_emptyIconDark ??= LoadEmbeddedIcon("empty_dark.ico")) : (_fullIconDark ??= LoadEmbeddedIcon("full_dark.ico"));

    private Icon LoadEmbeddedIcon(string iconName)
    {
        using var stream = GetType().Assembly.GetManifestResourceStream($"SimpleBin.Resources.{iconName}");
        if (stream != null)
        {
            return new Icon(stream);
        }

        // Return system default if embedded resource fails
        return SystemIcons.Application;
    }

    private bool IsRecycleBinEmpty()
    {
        try
        {
            var recycleBinPath = GetRecycleBinPath();
            if (!Directory.Exists(recycleBinPath))
                return true;

            // Quick check - if directory is empty, recycle bin is empty
            var files = Directory.GetFiles(recycleBinPath, "*", SearchOption.AllDirectories);
            return files.Length == 0;
        }
        catch
        {
            return true; // Assume empty if we can't check
        }
    }

    private static string GetRecycleBinPath()
    {
        // Get current user's SID for recycle bin path
        var userSid = System.Security.Principal.WindowsIdentity.GetCurrent().User?.ToString() ?? "";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows)[..3], "$Recycle.Bin", userSid);
    }

    private static bool IsSystemDarkTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("SystemUsesLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false; // Default to light theme
        }
    }

    private void OnNotifyIconClick(object? sender, EventArgs e)
    {
        if (e is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
        {
            OpenRecycleBin();
        }
    }

    private void OnViewInfo(object? sender, EventArgs e) => _ = ShowRecycleBinInfoAsync();

    private static async Task ShowRecycleBinInfoAsync()
    {
        try
        {
            var (fileCount, totalSize) = await Task.Run(GetRecycleBinInfo);

            var message = "Recycle Bin Information:\n\n" +
                           $"Files: {fileCount:N0}\n" +
                           $"Total Size: {FormatBytes(totalSize)}";

            MessageBox.Show(message, "SimpleBin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error getting recycle bin info: {ex.Message}", "SimpleBin",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static (int fileCount, long totalSize) GetRecycleBinInfo()
    {
        try
        {
            var recycleBinPath = GetRecycleBinPath();
            if (!Directory.Exists(recycleBinPath))
                return (0, 0);

            var files = Directory.GetFiles(recycleBinPath, "*", SearchOption.AllDirectories);
            long totalSize = 0;

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                catch
                {
                    // Skip files we can't access
                }
            }

            return (files.Length, totalSize);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:N1} {suffixes[suffixIndex]}";
    }

    private void OnEmptyRecycleBin(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to permanently delete all items in the Recycle Bin?",
            "Empty Recycle Bin",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            _ = EmptyRecycleBinAsync();
        }
    }

    private static async Task EmptyRecycleBinAsync()
    {
        try
        {
            await Task.Run(() => EmptyRecycleBin());
            MessageBox.Show("Recycle Bin emptied successfully.", "SimpleBin",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error emptying recycle bin: {ex.Message}", "SimpleBin",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void EmptyRecycleBin() => SHEmptyRecycleBin(IntPtr.Zero, null, 0);

    private static void OpenRecycleBin()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening recycle bin: {ex.Message}", "SimpleBin",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSettings(object? sender, EventArgs e) => new MainWindow().Show();

    private void OnExit(object? sender, EventArgs e) => ExitThread();

    private static void TrimWorkingSet()
    {
        try
        {
            // Trim working set to reduce memory usage
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch
        {
            // Ignore if trimming fails
        }
    }

    protected override void ExitThreadCore()
    {
        // Cleanup resources
        _cancellationTokenSource?.Cancel();
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _recycleBinWatcher?.Dispose();
        _notifyIcon?.Dispose();
        _contextMenu?.Dispose();

        // Don't dispose cached icons as they might be in use

        base.ExitThreadCore();
    }

    // P/Invoke declarations
    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
}
