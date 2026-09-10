// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
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
        Colour4 Regular { get; }
        Colour4 Dark { get; }
        Colour4 Darker { get; }
        Colour4 Darkest { get; }
        Colour4 Light { get; }
        Colour4 Lighter { get; }
        Colour4 Lightest { get; }
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
                : this(Colour4.FromHex(regular), Colour4.FromHex(dark), Colour4.FromHex(darker), Colour4.FromHex(darkest), Colour4.FromHex(light), Colour4.FromHex(lighter), Colour4.FromHex(lightest))
            {
            }

            public FumoColour(Colour4 regular, Colour4 dark, Colour4 darker, Colour4 darkest, Colour4 light, Colour4 lighter, Colour4 lightest)
            {
                Regular = regular;
                Dark = dark;
                Darker = darker;
                Darkest = darkest;
                Light = light;
                Lighter = lighter;
                Lightest = lightest;
            }

            public Colour4 Regular { get; }
            public Colour4 Dark { get; }
            public Colour4 Darker { get; }
            public Colour4 Darkest { get; }
            public Colour4 Light { get; }
            public Colour4 Lighter { get; }
            public Colour4 Lightest { get; }
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

        /// <summary>
        /// Derive a full <see cref="IFumoColour"/> scheme from a single theme colour.
        /// </summary>
        /// <remarks>
        /// Hue and saturation are kept unchanged; every shade is produced purely by shifting lightness,
        /// following the progression of the predefined themes (which mirrors the Material Design
        /// 500 / 600-800 / 400-200 steps). Lightness deltas are clamped, so very dark or very light
        /// theme colours will produce a compressed but still valid scheme.
        /// </remarks>
        /// <param name="regular">the theme colour, used as the <see cref="IFumoColour.Regular"/> shade</param>
        public static IFumoColour FromThemeColour(Colour4 regular) => new FumoColour(
            regular,
            deriveShade(regular, -0.05f),
            deriveShade(regular, -0.09f),
            deriveShade(regular, -0.15f),
            deriveShade(regular, +0.07f),
            deriveShade(regular, +0.15f),
            deriveShade(regular, +0.23f)
        );

        private static Colour4 deriveShade(Colour4 colour, float lightnessDelta)
        {
            var hsl = colour.ToHSL();
            return Colour4.FromHSL(hsl.X, hsl.Y, Math.Clamp(hsl.Z + lightnessDelta, 0f, 1f));
        }
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
        public readonly Colour4 Accent;
        public readonly Colour4 Background;
        public readonly Colour4 TriangleLight;
        public readonly Colour4 TriangleDark;

        public ModColourScheme(Colour4 accent, Colour4 background, Colour4 triangleLight, Colour4 triangleDark)
        {
            Accent = accent;
            Background = background;
            TriangleLight = triangleLight;
            TriangleDark = triangleDark;
        }

        /// <summary>
        /// Derive a <see cref="ModColourScheme"/> from a single theme colour, used as the <see cref="Accent"/>.
        /// </summary>
        /// <remarks>
        /// All derived colours reuse the accent's hue with reduced saturation and low lightness,
        /// following the pattern of the predefined schemes (accent is vivid, background is dark,
        /// and the two triangle colours sit slightly above the background lightness).
        /// </remarks>
        /// <param name="accent">the theme colour</param>
        public static ModColourScheme FromThemeColour(Colour4 accent)
        {
            var hsl = accent.ToHSL();
            float hue = hsl.X;
            float saturation = Math.Clamp(hsl.Y * 0.5f, 0.1f, 0.65f);

            return new ModColourScheme(
                accent,
                Colour4.FromHSL(hue, saturation, 0.20f),
                Colour4.FromHSL(hue, saturation, 0.25f),
                Colour4.FromHSL(hue, saturation, 0.22f)
            );
        }
    }
}
