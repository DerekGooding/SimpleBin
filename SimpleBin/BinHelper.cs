using System.Runtime.InteropServices;

namespace SimpleBin;

static partial class NativeBinMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Shqueryrbinfo
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int SHQueryRecycleBinW(
         string? pszRootPath,
         ref Shqueryrbinfo pShQueryRbInfo);

    [Flags]
    internal enum RecycleBinFlags : uint
    {
        None = 0,
        SherbNoconfirmation = 0x00000001,
        SherbNoprogressui = 0x00000002,
        SherbNosound = 0x00000004
    }

    [LibraryImport("shell32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int SHEmptyRecycleBinW(
         IntPtr hwnd,
         string? pszRootPath,
         RecycleBinFlags dwFlags);
}

public static class BinHelper
{
    internal static (long biteSize, long itemCount) GetBinSize()
    {
        const string? pszRootPath = null; //need to watch size from all disks
        var info = new NativeBinMethods.Shqueryrbinfo();
        info.cbSize = Marshal.SizeOf(info);
        const int okCode = 0;

        var result = NativeBinMethods.SHQueryRecycleBinW(pszRootPath, ref info);

        return result != okCode ? throw new Exception("SHQueryRecycleBinW failed") : ((long biteSize, long itemCount))(info.i64Size, info.i64NumItems);
    }

    internal static bool ClearBin()
    {
        const int okCode = 0;
        var parentWindow = IntPtr.Zero;
        const string? pszRootPath = null; //need to clear data from all disks

        const NativeBinMethods.RecycleBinFlags flags = NativeBinMethods.RecycleBinFlags.SherbNoconfirmation |
                                NativeBinMethods.RecycleBinFlags.SherbNosound;

        var resultCode = NativeBinMethods.SHEmptyRecycleBinW(parentWindow, pszRootPath, flags);

        return resultCode != okCode ? throw new Exception("SHEmptyRecycleBinW failed") : true;
    }
    public static bool IsBinEmpty() => GetBinSize().itemCount == 0;
}