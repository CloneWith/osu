// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    /// <summary>
    /// Useful colour schemes for our customized interface.
    /// </summary>
    /// <remarks>Some of these schemes are inspired from Material UI.</remarks>
    public static class FumoColours
    {
        public static class SeaBlue
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex(@"#269FFE");
            public static Color4 Dark { get; } = Color4Extensions.FromHex(@"#0992FE");
            public static Color4 Darker { get; } = Color4Extensions.FromHex(@"#018BF7");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex(@"#0172CB");
            public static Color4 Light { get; } = Color4Extensions.FromHex(@"#43ACFE");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex(@"#52B2FE");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex(@"#7DC6FE");

            public static Color4[] ColourSet => [Regular, Dark, Darker, Darkest, Light, Lighter, Lightest];
        }

        public static class SunshineYellow
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex(@"#F29F00");
            public static Color4 Dark { get; } = Color4Extensions.FromHex(@"#F18F00");
            public static Color4 Darker { get; } = Color4Extensions.FromHex(@"#ED8300");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex(@"#E87300");
            public static Color4 Light { get; } = Color4Extensions.FromHex(@"#F4B03B");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex(@"#F7C775");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex(@"#FADDAC");

            public static Color4[] ColourSet => [Regular, Dark, Darker, Darkest, Light, Lighter, Lightest];
        }

        public static class FlandreRed
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex(@"#D05677");
            public static Color4 Dark { get; } = Color4Extensions.FromHex(@"#C94562");
            public static Color4 Darker { get; } = Color4Extensions.FromHex(@"#B9415E");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex(@"#A33C58");
            public static Color4 Light { get; } = Color4Extensions.FromHex(@"#D96E8D");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex(@"#E394AC");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex(@"#EEBDCD");

            public static Color4[] ColourSet => [Regular, Dark, Darker, Darkest, Light, Lighter, Lightest];
        }

        public static class DeepPurple
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex(@"#673AB7");
            public static Color4 Dark { get; } = Color4Extensions.FromHex(@"#5E35B1");
            public static Color4 Darker { get; } = Color4Extensions.FromHex(@"#512DA8");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex(@"#4527A0");
            public static Color4 Light { get; } = Color4Extensions.FromHex(@"#7E57C2");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex(@"#9575CD");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex(@"#B39DDB");

            public static Color4[] ColourSet => [Regular, Dark, Darker, Darkest, Light, Lighter, Lightest];
        }

        public static class LightGreen
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex(@"#8BC34A");
            public static Color4 Dark { get; } = Color4Extensions.FromHex(@"#7CB342");
            public static Color4 Darker { get; } = Color4Extensions.FromHex(@"#689F38");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex(@"#558B2F");
            public static Color4 Light { get; } = Color4Extensions.FromHex(@"#9CCC65");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex(@"#AED581");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex(@"#C5E1A5");

            public static Color4[] ColourSet => [Regular, Dark, Darker, Darkest, Light, Lighter, Lightest];
        }
    }

    /// <summary>
    /// All mod colour schemes used by chess pieces.
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

        /// <summary>
        /// The fallback colour scheme for empty and unavailable chess pieces.
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
        /// <returns>a colour scheme of the mod, <see cref="Empty"/> if unavailable</returns>
        public static ModColourScheme FromModString(string mod) => mod.Trim().ToUpperInvariant() switch
        {
            @"NM" => NoMod,
            @"HR" => HardRock,
            @"HD" => Hidden,
            @"DT" => DoubleTime,
            @"FM" => FreeMod,
            _ => Empty
        };
    }

    /// <summary>
    /// A set of colours for a specific mod.
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
