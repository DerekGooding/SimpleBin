using System.Reflection;
using System.Resources;

namespace SimpleBin;

public class IconHelper
{
    private ResourceManager _resourceManager;

#pragma warning disable CS8618
    public IconHelper(bool isDarkTheme) => SetTheme(isDarkTheme);

    private static string ResourseNameGenerator(bool isDarkTheme) => $"SimpleBin.Resources.{(isDarkTheme ? "DarkIcons" : "LightIcons")}";

    public Icon GetEmptyIcon() => (Icon)_resourceManager.GetObject("EmptyBin")!;

    public Icon GetIcon() => (Icon)_resourceManager.GetObject("Bin")!;

    public void SetTheme(bool isDarkTheme)
    => _resourceManager = new(ResourseNameGenerator(isDarkTheme), Assembly.GetExecutingAssembly());
}
