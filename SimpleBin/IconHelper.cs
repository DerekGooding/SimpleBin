using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Resources;

namespace SimpleBin;

public class IconHelper
{
    private ResourceManager _resourceManager;
    // Cache icons instead of recreating them
    private static Icon? _emptyIcon;
    private static Icon? _fullIcon;

#pragma warning disable CS8618
    public IconHelper(bool isDarkTheme) => SetTheme(isDarkTheme);

    private static string ResourseNameGenerator(bool isDarkTheme) => $"SimpleBin.Resources.{(isDarkTheme ? "DarkIcons" : "LightIcons")}";

    public Icon GetEmptyIcon() => GetCachedIcon(true);

    public Icon GetIcon() => GetCachedIcon(false);

    private Icon GetCachedIcon(bool isEmpty)
        => isEmpty ? (_emptyIcon ??= LoadEmbeddedIcon("EmptyBin")) : (_fullIcon ??= LoadEmbeddedIcon("Bin"));

    private Icon LoadEmbeddedIcon(string iconName) => (Icon)_resourceManager.GetObject(iconName)!;

    public void SetTheme(bool isDarkTheme)
    => _resourceManager = new(ResourseNameGenerator(isDarkTheme), Assembly.GetExecutingAssembly());
}
