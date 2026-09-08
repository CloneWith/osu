// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    /// <summary>
    /// Represents a color theme with a few variants.
    /// </summary>
    public interface IFumoColour
    {
        Color4 Regular { get; }
        Color4 Dark { get; }
        Color4 Darker { get; }
        Color4 Darkest { get; }
        Color4 Light { get; }
        Color4 Lighter { get; }
        Color4 Lightest { get; }
    }

    /// <summary>
    /// Useful color schemes for our customized interface.
    /// </summary>
    /// <remarks>Some of these schemes are inspired from Material UI.</remarks>
    public static class FumoColours
    {
        private sealed class FumoColour : IFumoColour
        {
            public FumoColour(string regular, string dark, string darker, string darkest, string light, string lighter, string lightest)
            {
                Regular = Color4Extensions.FromHex(regular);
                Dark = Color4Extensions.FromHex(dark);
                Darker = Color4Extensions.FromHex(darker);
                Darkest = Color4Extensions.FromHex(darkest);
                Light = Color4Extensions.FromHex(light);
                Lighter = Color4Extensions.FromHex(lighter);
                Lightest = Color4Extensions.FromHex(lightest);
            }

            public Color4 Regular { get; }
            public Color4 Dark { get; }
            public Color4 Darker { get; }
            public Color4 Darkest { get; }
            public Color4 Light { get; }
            public Color4 Lighter { get; }
            public Color4 Lightest { get; }
        }

        public static IFumoColour SeaBlue { get; } = new FumoColour(@"#269FFE", @"#0992FE", @"#018BF7", @"#0172CB", @"#43ACFE", @"#52B2FE", @"#7DC6FE");
        public static IFumoColour SunshineYellow { get; } = new FumoColour(@"#F29F00", @"#F18F00", @"#ED8300", @"#E87300", @"#F4B03B", @"#F7C775", @"#FADDAC");
        public static IFumoColour FlandreRed { get; } = new FumoColour(@"#D05677", @"#C94562", @"#B9415E", @"#A33C58", @"#D96E8D", @"#E394AC", @"#EEBDCD");
        public static IFumoColour DeepPurple { get; } = new FumoColour(@"#673AB7", @"#5E35B1", @"#512DA8", @"#4527A0", @"#7E57C2", @"#9575CD", @"#B39DDB");
        public static IFumoColour LightGreen { get; } = new FumoColour(@"#8BC34A", @"#7CB342", @"#689F38", @"#558B2F", @"#9CCC65", @"#AED581", @"#C5E1A5");

        /// <summary>
        /// Get the represented <see cref="IFumoColour"/> from a theme.
        /// </summary>
        /// <param name="theme">the color theme</param>
        public static IFumoColour FromTheme(FumoColourScheme theme) => theme switch
        {
            FumoColourScheme.SeaBlue => SeaBlue,
            FumoColourScheme.SunshineYellow => SunshineYellow,
            FumoColourScheme.FlandreRed => FlandreRed,
            FumoColourScheme.DeepPurple => DeepPurple,
            FumoColourScheme.LightGreen => LightGreen,
            _ => SeaBlue,
        };
    }

    /// <summary>
    /// Currently available color themes.
    /// </summary>
    public enum FumoColourScheme
    {
        [LocalisableDescription(typeof(FumoColourStrings), nameof(FumoColourStrings.SeaBlue))]
        SeaBlue,

        [LocalisableDescription(typeof(FumoColourStrings), nameof(FumoColourStrings.SunshineYellow))]
        SunshineYellow,

        [LocalisableDescription(typeof(FumoColourStrings), nameof(FumoColourStrings.FlandreRed))]
        FlandreRed,

        [LocalisableDescription(typeof(FumoColourStrings), nameof(FumoColourStrings.DeepPurple))]
        DeepPurple,

        [LocalisableDescription(typeof(FumoColourStrings), nameof(FumoColourStrings.LightGreen))]
        LightGreen,
    }

    /// <summary>
    /// All mod color schemes used by chess pieces.
    /// </summary>
    public static class ModColours
    {
        public static ModColourScheme NoMod = new ModColourScheme(
            Color4Extensions.FromHex(@"#FFEB3B"), Color4Extensions.FromHex(@"#534D1E"),
            Color4Extensions.FromHex(@"#645D25"), Color4Extensions.FromHex(@"#56501F"));

        public static ModColourScheme HardRock = new ModColourScheme(
            Color4Extensions.FromHex(@"#FF5733"), Color4Extensions.FromHex(@"#3B180F"),
            Color4Extensions.FromHex(@"#5E2719"), Color4Extensions.FromHex(@"#4C2014"));

        public static ModColourScheme Hidden = new ModColourScheme(
            Color4Extensions.FromHex(@"#FF8D1A"), Color4Extensions.FromHex(@"#472C10"),
            Color4Extensions.FromHex(@"#5C3815"), Color4Extensions.FromHex(@"#4F3011"));

        public static ModColourScheme DoubleTime = new ModColourScheme(
            Color4Extensions.FromHex(@"#9D73FF"), Color4Extensions.FromHex(@"#31264F"),
            Color4Extensions.FromHex(@"#3C2F63"), Color4Extensions.FromHex(@"#322751"));

        public static ModColourScheme FreeMod = new ModColourScheme(
            Color4Extensions.FromHex(@"#43CF7C"), Color4Extensions.FromHex(@"#203D27"),
            Color4Extensions.FromHex(@"#2A4F33"), Color4Extensions.FromHex(@"#23432B"));

        public static ModColourScheme TieBreaker = new ModColourScheme(
            Color4Extensions.FromHex(@"#FFA500"), Color4Extensions.FromHex(@"#714800"),
            Color4Extensions.FromHex(@"#4C3000"), Color4Extensions.FromHex(@"#986000"));

        /// <summary>
        /// The fallback color scheme for empty and unavailable chess pieces.
        /// </summary>
        public static ModColourScheme Empty = new ModColourScheme(
            Color4.White, Color4Extensions.FromHex(@"#BDBDBD"),
            Color4Extensions.FromHex(@"#E5E5E5"), Color4Extensions.FromHex(@"#C7C7C7"));

        public static ModColourScheme Consumed = new ModColourScheme(
            Color4.White, Color4Extensions.FromHex(@"#545454"),
            Color4Extensions.FromHex(@"#666666"), Color4Extensions.FromHex(@"#5F5F5F"));

        public static ModColourScheme RedWin = new ModColourScheme(
            Color4Extensions.FromHex(@"#FF5733"), Color4Extensions.FromHex(@"#4D2114"),
            Color4Extensions.FromHex(@"#5E2719"), Color4Extensions.FromHex(@"#522215"));

        public static ModColourScheme BlueWin = new ModColourScheme(
            Color4Extensions.FromHex(@"#57C1FF"), Color4Extensions.FromHex(@"#263E52"),
            Color4Extensions.FromHex(@"#2E4C64"), Color4Extensions.FromHex(@"#274257"));

        /// <summary>
        /// Get the corresponding <see cref="ModColourScheme"/> based on a given string.
        /// </summary>
        /// <param name="mod">the given mod string</param>
        /// <returns>a color scheme of the mod, <see cref="Empty"/> if unavailable</returns>
        public static ModColourScheme FromModString(string? mod) => mod?.Trim().ToUpperInvariant() switch
        {
            @"NM" => NoMod,
            @"HR" => HardRock,
            @"HD" => Hidden,
            @"DT" => DoubleTime,
            @"FM" => FreeMod,
            @"TB" => TieBreaker,
            _ => Empty
        };
    }

    /// <summary>
    /// A set of colors for a specific mod.
    /// </summary>
    public class ModColourScheme
    {
        public readonly Color4 Accent;
        public readonly Color4 Background;
        public readonly Color4 TriangleLight;
        public readonly Color4 TriangleDark;

        public ModColourScheme(Color4 accent, Color4 background, Color4 triangleLight, Color4 triangleDark)
        {
            Accent = accent;
            Background = background;
            TriangleLight = triangleLight;
            TriangleDark = triangleDark;
        }
    }
}
