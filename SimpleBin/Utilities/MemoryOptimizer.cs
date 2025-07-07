using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Timer = System.Threading.Timer;

namespace SimpleBin.Utilities;

/// <summary>
/// Helper class for memory optimization in tray applications
/// </summary>
public static partial class MemoryOptimizer
{
    private static Timer? _memoryTrimTimer;
    private static readonly Lock _lockObject = new();
    private static bool _isOptimizing = false;

    // Memory pressure thresholds (in MB)
    private const long HIGH_MEMORY_THRESHOLD = 50 * 1024 * 1024; // 50MB
    private const long CRITICAL_MEMORY_THRESHOLD = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Initialize memory optimization with periodic cleanup
    /// </summary>
    public static void Initialize()
    {
        // Setup periodic memory trimming (every 5 minutes)
        _memoryTrimTimer = new Timer(OnMemoryTrimTimer, null,
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        // Configure GC for server-optimized collection
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
    }

    /// <summary>
    /// Perform immediate memory optimization
    /// </summary>
    public static void OptimizeNow()
    {
        if (_isOptimizing) return;

        lock (_lockObject)
        {
            if (_isOptimizing) return;
            _isOptimizing = true;
        }

        try
        {
            // Get current memory usage
            var memoryBefore = GetCurrentMemoryUsage();

            // Perform optimization
            PerformMemoryOptimization();

            // Check results
            var memoryAfter = GetCurrentMemoryUsage();
            var savedMemory = memoryBefore - memoryAfter;

            Debug.WriteLine($"Memory optimization completed. Saved: {FormatBytes(savedMemory)}");
        }
        finally
        {
            _isOptimizing = false;
        }
    }

    /// <summary>
    /// Optimize memory usage asynchronously
    /// </summary>
    public static async Task OptimizeAsync() => await Task.Run(() => OptimizeNow());

    /// <summary>
    /// Check if system is under memory pressure
    /// </summary>
    public static bool IsUnderMemoryPressure()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;

            return workingSet > HIGH_MEMORY_THRESHOLD;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get current memory usage in bytes
    /// </summary>
    public static long GetCurrentMemoryUsage()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            return process.WorkingSet64;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get memory usage statistics
    /// </summary>
    public static MemoryStats GetMemoryStats()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            return new MemoryStats
            {
                WorkingSet = process.WorkingSet64,
                PrivateMemorySize = process.PrivateMemorySize64,
                VirtualMemorySize = process.VirtualMemorySize64,
                GcTotalMemory = GC.GetTotalMemory(false),
                GcGen0Collections = GC.CollectionCount(0),
                GcGen1Collections = GC.CollectionCount(1),
                GcGen2Collections = GC.CollectionCount(2)
            };
        }
        catch
        {
            return new MemoryStats();
        }
    }

    /// <summary>
    /// Force immediate garbage collection and memory trimming
    /// </summary>
    public static void ForceCleanup()
    {
        // Force full garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Trim working set
        TrimWorkingSet();

        // Compact large object heap
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }

    /// <summary>
    /// Trim the working set to reduce memory usage
    /// </summary>
    public static void TrimWorkingSet()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            SetProcessWorkingSetSize(process.Handle, -1, -1);
        }
        catch
        {
            // Ignore if trimming fails
        }
    }

    /// <summary>
    /// Configure GC for minimal memory usage
    /// </summary>
    public static void ConfigureForMinimalMemory()
    {
        // Set GC to optimize for memory rather than throughput
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

        // Enable server GC if available (better for memory management)
        if (GCSettings.IsServerGC)
        {
            Debug.WriteLine("Server GC is enabled - good for memory management");
        }
    }

    /// <summary>
    /// Monitor memory usage and perform cleanup when needed
    /// </summary>
    public static void StartMemoryMonitoring()
    => Task.Run(async () =>
    {
        while (true)
        {
            try
            {
                var currentMemory = GetCurrentMemoryUsage();

                if (currentMemory > CRITICAL_MEMORY_THRESHOLD)
                {
                    Debug.WriteLine($"Critical memory usage detected: {FormatBytes(currentMemory)}");
                    ForceCleanup();
                }
                else if (currentMemory > HIGH_MEMORY_THRESHOLD)
                {
                    Debug.WriteLine($"High memory usage detected: {FormatBytes(currentMemory)}");
                    OptimizeNow();
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
            catch
            {
                // Continue monitoring even if there's an error
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    });

    private static void OnMemoryTrimTimer(object? state)
    {
        if (!IsUnderMemoryPressure()) return;

        OptimizeNow();
    }

    private static void PerformMemoryOptimization()
    {
        // Step 1: Garbage collection
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true);

        // Step 2: Trim working set
        TrimWorkingSet();

        // Step 3: Compact large object heap if needed
        var totalMemory = GC.GetTotalMemory(false);
        if (totalMemory > 10 * 1024 * 1024) // 10MB
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0) return "0 B";

        string[] suffixes = ["B", "KB", "MB", "GB"];
        double value = bytes;
        var suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:N1} {suffixes[suffixIndex]}";
    }

    /// <summary>
    /// Cleanup resources when shutting down
    /// </summary>
    public static void Dispose()
    {
        _memoryTrimTimer?.Dispose();
        _memoryTrimTimer = null;
    }

    // P/Invoke declarations
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
}

/// <summary>
/// Memory usage statistics
/// </summary>
public struct MemoryStats
{
    public long WorkingSet { get; set; }
    public long PrivateMemorySize { get; set; }
    public long VirtualMemorySize { get; set; }
    public long GcTotalMemory { get; set; }
    public int GcGen0Collections { get; set; }
    public int GcGen1Collections { get; set; }
    public int GcGen2Collections { get; set; }

    public override readonly string ToString()
        => $"Working Set: {FormatBytes(WorkingSet)}, " +
           $"Private: {FormatBytes(PrivateMemorySize)}, " +
           $"GC Memory: {FormatBytes(GcTotalMemory)}, " +
           $"GC Collections: {GcGen0Collections}/{GcGen1Collections}/{GcGen2Collections}";

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0) return "0 B";

        string[] suffixes = ["B", "KB", "MB", "GB"];
        double value = bytes;
        var suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:N1} {suffixes[suffixIndex]}";
    }
}