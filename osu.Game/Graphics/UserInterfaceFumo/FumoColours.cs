// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    /// <summary>
    /// Useful colour schemes for our customized interface.
    /// <br />
    /// Some of them are inspired from Material UI.
    /// </summary>
    public static class FumoColours
    {
        public static class SeaBlue
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex("#269FFE");
            public static Color4 Dark { get; } = Color4Extensions.FromHex("#0992FE");
            public static Color4 Darker { get; } = Color4Extensions.FromHex("#018BF7");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex("#0172CB");
            public static Color4 Light { get; } = Color4Extensions.FromHex("#43ACFE");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex("#52B2FE");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex("#7DC6FE");
        }

        public static class FlandreRed
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex("#D05677");
            public static Color4 Dark { get; } = Color4Extensions.FromHex("#C94562");
            public static Color4 Darker { get; } = Color4Extensions.FromHex("#B9415E");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex("#A33C58");
            public static Color4 Light { get; } = Color4Extensions.FromHex("#D96E8D");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex("#E394AC");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex("#EEBDCD");
        }

        public static class DeepPurple
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex("#673AB7");
            public static Color4 Dark { get; } = Color4Extensions.FromHex("#5E35B1");
            public static Color4 Darker { get; } = Color4Extensions.FromHex("#512DA8");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex("#4527A0");
            public static Color4 Light { get; } = Color4Extensions.FromHex("#7E57C2");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex("#9575CD");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex("#B39DDB");
        }

        public static class LightGreen
        {
            public static Color4 Regular { get; } = Color4Extensions.FromHex("#8BC34A");
            public static Color4 Dark { get; } = Color4Extensions.FromHex("#7CB342");
            public static Color4 Darker { get; } = Color4Extensions.FromHex("#689F38");
            public static Color4 Darkest { get; } = Color4Extensions.FromHex("#558B2F");
            public static Color4 Light { get; } = Color4Extensions.FromHex("#9CCC65");
            public static Color4 Lighter { get; } = Color4Extensions.FromHex("#AED581");
            public static Color4 Lightest { get; } = Color4Extensions.FromHex("#C5E1A5");
        }
    }
}
