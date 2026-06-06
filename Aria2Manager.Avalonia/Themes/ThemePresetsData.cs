using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using System.Collections.Generic;

namespace Aria2Manager.Avalonia.Themes
{
    public static class ThemePresetsData
    {
        public static IEnumerable<ThemePreset> GetDefaultPresets()
        {
            yield return new ThemePreset
            {
                Name = "Default",
                LightPalette = new ColorPaletteResources(),
                DarkPalette = new ColorPaletteResources()
            };
            yield return new ThemePreset
            {
                Name = "Avalonia",
                LightPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ff0073cf"),
                    AltHigh = Colors.White,
                    AltLow = Colors.White,
                    AltMedium = Colors.White,
                    AltMediumHigh = Colors.White,
                    AltMediumLow = Colors.White,
                    BaseHigh = Colors.Black,
                    BaseLow = Color.Parse("#ffcccccc"),
                    BaseMedium = Color.Parse("#ff898989"),
                    BaseMediumHigh = Color.Parse("#ff5d5d5d"),
                    BaseMediumLow = Color.Parse("#ff737373"),
                    ChromeAltLow = Color.Parse("#ff5d5d5d"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffcccccc"),
                    ChromeBlackMedium = Color.Parse("#ff5d5d5d"),
                    ChromeBlackMediumLow = Color.Parse("#ff898989"),
                    ChromeDisabledHigh = Color.Parse("#ffcccccc"),
                    ChromeDisabledLow = Color.Parse("#ff898989"),
                    ChromeGray = Color.Parse("#ff737373"),
                    ChromeHigh = Color.Parse("#ffcccccc"),
                    ChromeLow = Color.Parse("#ffececec"),
                    ChromeMedium = Color.Parse("#ffe6e6e6"),
                    ChromeMediumLow = Color.Parse("#ffececec"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ffe6e6e6"),
                    ListMedium = Color.Parse("#ffcccccc"),
                    RegionColor = Colors.White
                },
                DarkPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ff0073cf"),
                    AltHigh = Colors.Black,
                    AltLow = Colors.Black,
                    AltMedium = Colors.Black,
                    AltMediumHigh = Colors.Black,
                    AltMediumLow = Colors.Black,
                    BaseHigh = Colors.White,
                    BaseLow = Color.Parse("#ff333333"),
                    BaseMedium = Color.Parse("#ff9a9a9a"),
                    BaseMediumHigh = Color.Parse("#ffb4b4b4"),
                    BaseMediumLow = Color.Parse("#ff676767"),
                    ChromeAltLow = Color.Parse("#ffb4b4b4"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffb4b4b4"),
                    ChromeBlackMedium = Colors.Black,
                    ChromeBlackMediumLow = Colors.Black,
                    ChromeDisabledHigh = Color.Parse("#ff333333"),
                    ChromeDisabledLow = Color.Parse("#ff9a9a9a"),
                    ChromeGray = Colors.Gray,
                    ChromeHigh = Colors.Gray,
                    ChromeLow = Color.Parse("#ff151515"),
                    ChromeMedium = Color.Parse("#ff1d1d1d"),
                    ChromeMediumLow = Color.Parse("#ff2c2c2c"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ff1d1d1d"),
                    ListMedium = Color.Parse("#ff333333"),
                    RegionColor = Colors.Black
                }
            };
            yield return new ThemePreset
            {
                Name = "Lavender",
                LightPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ff8961cc"),
                    AltHigh = Colors.White,
                    AltLow = Colors.White,
                    AltMedium = Colors.White,
                    AltMediumHigh = Colors.White,
                    AltMediumLow = Colors.White,
                    BaseHigh = Colors.Black,
                    BaseLow = Color.Parse("#ffeeceff"),
                    BaseMedium = Color.Parse("#ffa987bc"),
                    BaseMediumHigh = Color.Parse("#ff7b5890"),
                    BaseMediumLow = Color.Parse("#ff9270a6"),
                    ChromeAltLow = Color.Parse("#ff7b5890"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffeeceff"),
                    ChromeBlackMedium = Color.Parse("#ff7b5890"),
                    ChromeBlackMediumLow = Color.Parse("#ffa987bc"),
                    ChromeDisabledHigh = Color.Parse("#ffeeceff"),
                    ChromeDisabledLow = Color.Parse("#ffa987bc"),
                    ChromeGray = Color.Parse("#ff9270a6"),
                    ChromeHigh = Color.Parse("#ffeeceff"),
                    ChromeLow = Color.Parse("#fffeeaff"),
                    ChromeMedium = Color.Parse("#fffbe4ff"),
                    ChromeMediumLow = Color.Parse("#fffeeaff"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#fffbe4ff"),
                    ListMedium = Color.Parse("#ffeeceff"),
                    RegionColor = Color.Parse("#fffef6ff")
                },
                DarkPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ff8961cc"),
                    AltHigh = Colors.Black,
                    AltLow = Colors.Black,
                    AltMedium = Colors.Black,
                    AltMediumHigh = Colors.Black,
                    AltMediumLow = Colors.Black,
                    BaseHigh = Colors.White,
                    BaseLow = Color.Parse("#ff64576b"),
                    BaseMedium = Color.Parse("#ffb6aabc"),
                    BaseMediumHigh = Color.Parse("#ffcbbfd0"),
                    BaseMediumLow = Color.Parse("#ff8d8193"),
                    ChromeAltLow = Color.Parse("#ffcbbfd0"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffcbbfd0"),
                    ChromeBlackMedium = Colors.Black,
                    ChromeBlackMediumLow = Colors.Black,
                    ChromeDisabledHigh = Color.Parse("#ff64576b"),
                    ChromeDisabledLow = Color.Parse("#ffb6aabc"),
                    ChromeGray = Color.Parse("#ffa295a8"),
                    ChromeHigh = Color.Parse("#ffa295a8"),
                    ChromeLow = Color.Parse("#ff332041"),
                    ChromeMedium = Color.Parse("#ff3f2e4b"),
                    ChromeMediumLow = Color.Parse("#ff584960"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ff3f2e4b"),
                    ListMedium = Color.Parse("#ff64576b"),
                    RegionColor = Color.Parse("#ff262738")
                }
            };
            yield return new ThemePreset
            {
                Name = "Forest",
                LightPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ff34854d"),
                    AltHigh = Colors.White,
                    AltLow = Colors.White,
                    AltMedium = Colors.White,
                    AltMediumHigh = Colors.White,
                    AltMediumLow = Colors.White,
                    BaseHigh = Colors.Black,
                    BaseLow = Color.Parse("#ffc2db65"),
                    BaseMedium = Color.Parse("#ff7d9728"),
                    BaseMediumHigh = Color.Parse("#ff4f6a00"),
                    BaseMediumLow = Color.Parse("#ff668114"),
                    ChromeAltLow = Color.Parse("#ff4f6a00"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffc2db65"),
                    ChromeBlackMedium = Color.Parse("#ff4f6a00"),
                    ChromeBlackMediumLow = Color.Parse("#ff7d9728"),
                    ChromeDisabledHigh = Color.Parse("#ffc2db65"),
                    ChromeDisabledLow = Color.Parse("#ff7d9728"),
                    ChromeGray = Color.Parse("#ff668114"),
                    ChromeHigh = Color.Parse("#ffc2db65"),
                    ChromeLow = Color.Parse("#ffe6f3bb"),
                    ChromeMedium = Color.Parse("#ffdfeeaa"),
                    ChromeMediumLow = Color.Parse("#ffe6f3bb"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ffdfeeaa"),
                    ListMedium = Color.Parse("#ffc2db65"),
                    RegionColor = Color.Parse("#fff7ffff")
                },
                DarkPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ff34854d"),
                    AltHigh = Colors.Black,
                    AltLow = Colors.Black,
                    AltMedium = Colors.Black,
                    AltMediumHigh = Colors.Black,
                    AltMediumLow = Colors.Black,
                    BaseHigh = Colors.White,
                    BaseLow = Color.Parse("#ff784834"),
                    BaseMedium = Color.Parse("#ffc5a294"),
                    BaseMediumHigh = Color.Parse("#ffd8b8ac"),
                    BaseMediumLow = Color.Parse("#ff9e7564"),
                    ChromeAltLow = Color.Parse("#ffd8b8ac"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffd8b8ac"),
                    ChromeBlackMedium = Colors.Black,
                    ChromeBlackMediumLow = Colors.Black,
                    ChromeDisabledHigh = Color.Parse("#ff784834"),
                    ChromeDisabledLow = Color.Parse("#ffc5a294"),
                    ChromeGray = Color.Parse("#ffb28b7c"),
                    ChromeHigh = Color.Parse("#ffb28b7c"),
                    ChromeLow = Color.Parse("#ff46150a"),
                    ChromeMedium = Color.Parse("#ff532215"),
                    ChromeMediumLow = Color.Parse("#ff6c3b2a"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ff532215"),
                    ListMedium = Color.Parse("#ff784834"),
                    RegionColor = Color.Parse("#ff353819")
                }
            };
            yield return new ThemePreset
            {
                Name = "Nighttime",
                LightPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ffcc4d11"),
                    AltHigh = Colors.White,
                    AltLow = Colors.White,
                    AltMedium = Colors.White,
                    AltMediumHigh = Colors.White,
                    AltMediumLow = Colors.White,
                    BaseHigh = Colors.Black,
                    BaseLow = Color.Parse("#ff7cbee0"),
                    BaseMedium = Color.Parse("#ff3282a8"),
                    BaseMediumHigh = Color.Parse("#ff005a83"),
                    BaseMediumLow = Color.Parse("#ff196e96"),
                    ChromeAltLow = Color.Parse("#ff005a83"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ff7cbee0"),
                    ChromeBlackMedium = Color.Parse("#ff005a83"),
                    ChromeBlackMediumLow = Color.Parse("#ff3282a8"),
                    ChromeDisabledHigh = Color.Parse("#ff7cbee0"),
                    ChromeDisabledLow = Color.Parse("#ff3282a8"),
                    ChromeGray = Color.Parse("#ff196e96"),
                    ChromeHigh = Color.Parse("#ff7cbee0"),
                    ChromeLow = Color.Parse("#ffc1e9fe"),
                    ChromeMedium = Color.Parse("#ffb3e0f8"),
                    ChromeMediumLow = Color.Parse("#ffc1e9fe"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ffb3e0f8"),
                    ListMedium = Color.Parse("#ff7cbee0"),
                    RegionColor = Color.Parse("#ffcfeaff")
                },
                DarkPalette = new ColorPaletteResources
                {
                    Accent = Color.Parse("#ffcc4d11"),
                    AltHigh = Colors.Black,
                    AltLow = Colors.Black,
                    AltMedium = Colors.Black,
                    AltMediumHigh = Colors.Black,
                    AltMediumLow = Colors.Black,
                    BaseHigh = Colors.White,
                    BaseLow = Color.Parse("#ff2f7bad"),
                    BaseMedium = Color.Parse("#ff8dbfdf"),
                    BaseMediumHigh = Color.Parse("#ffa5d0ec"),
                    BaseMediumLow = Color.Parse("#ff5e9dc6"),
                    ChromeAltLow = Color.Parse("#ffa5d0ec"),
                    ChromeBlackHigh = Colors.Black,
                    ChromeBlackLow = Color.Parse("#ffa5d0ec"),
                    ChromeBlackMedium = Colors.Black,
                    ChromeBlackMediumLow = Colors.Black,
                    ChromeDisabledHigh = Color.Parse("#ff2f7bad"),
                    ChromeDisabledLow = Color.Parse("#ff8dbfdf"),
                    ChromeGray = Color.Parse("#ff76aed3"),
                    ChromeHigh = Color.Parse("#ff76aed3"),
                    ChromeLow = Color.Parse("#ff093b73"),
                    ChromeMedium = Color.Parse("#ff134b82"),
                    ChromeMediumLow = Color.Parse("#ff266b9f"),
                    ChromeWhite = Colors.White,
                    ListLow = Color.Parse("#ff134b82"),
                    ListMedium = Color.Parse("#ff2f7bad"),
                    RegionColor = Color.Parse("#ff0d2644")
                }
            };
        }
    }
}