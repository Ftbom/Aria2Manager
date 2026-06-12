using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using System.Collections.Generic;
using System.Linq;

namespace Aria2Manager.Avalonia.Themes
{
    public class ThemePreset
    {
        public string Name { get; set; } = string.Empty;
        public ColorPaletteResources LightPalette { get; set; } = new();
        public ColorPaletteResources DarkPalette { get; set; } = new();
    }
    public static class ThemeManager
    {
        public static Dictionary<string, ThemePreset> Presets { get; } = new();
        static ThemeManager()
        {
            //注册主题
            foreach (var preset in ThemePresetsData.GetDefaultPresets())
            {
                Presets[preset.Name] = preset;
            }
        }
        //应用主题
        public static void ApplyPreset(string name)
        {
            if (!Presets.TryGetValue(name, out var preset)) return;
            var app = Application.Current;
            if (app == null) return;
            var oldTheme = app.Styles.OfType<FluentTheme>().FirstOrDefault();
            if (oldTheme == null) return;
            //记录位置索引
            var index = app.Styles.IndexOf(oldTheme);
            //创建新的FluentTheme
            var newTheme = new FluentTheme();
            //注入调色板
            if (name != "System")
            {
                newTheme.Palettes[ThemeVariant.Light] = preset.LightPalette;
                newTheme.Palettes[ThemeVariant.Dark] = preset.DarkPalette;
            }
            //直接用新主题替换旧主题
            app.Styles[index] = newTheme;
        }
    }
}