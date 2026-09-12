// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    /// <summary>
    /// Shared utility constants and methods for the tournament part.
    /// </summary>
    public static class TournamentExtensions
    {
        public const int STREAM_AREA_WIDTH = 1366;
        public const int STREAM_AREA_HEIGHT = (int)(STREAM_AREA_WIDTH / ASPECT_RATIO);

        public const float ASPECT_RATIO = 16 / 9f;

        public const float SONGBAR_HEIGHT = 145 / 2f;

        /// <summary>
        /// The maximum allowable penalty points. Users with points more than this would be disqualified.
        /// </summary>
        public const int PUNISHMENT_THRESHOLD = 3;

        public const int RESERVED_BEATMAP_ID = -1;

        public const int BOARD_BEST_OF = 9;
        public const int TIE_BREAKER_ROUND = 17;

        public static readonly List<string> SPECIAL_MODS = ["HR", "HD", "DT", "FL"];

        public static readonly Color4 COLOUR_RED = FumoColours.FlandreRed.Regular;
        public static readonly Color4 COLOUR_BLUE = FumoColours.SeaBlue.Regular;
        public static readonly Color4 COLOUR_CHOICES = Color4.Orange;
        public static readonly Color4 COLOUR_NEUTRAL = Color4.White;

        public static readonly Color4 ELEMENT_BACKGROUND_COLOUR = Color4Extensions.FromHex("#fff");
        public static readonly Color4 ELEMENT_FOREGROUND_COLOUR = Color4Extensions.FromHex("#000");
        public static readonly Color4 TEXT_COLOUR = Color4Extensions.FromHex("#fff");

        /// <summary>
        /// A list of normal mods used in the map pool.
        /// </summary>
        public static readonly List<KeyValuePair<string, string>> MODS =
        [
            new KeyValuePair<string, string>(@"NM", @"No Mod"),
            new KeyValuePair<string, string>(@"HD", @"Hidden"),
            new KeyValuePair<string, string>(@"HR", @"Hard Rock"),
            new KeyValuePair<string, string>(@"DT", @"Double Time"),
            new KeyValuePair<string, string>(@"FM", @"Free Mod"),
        ];

        /// <summary>
        /// Gets the opposite <see cref="TeamColour"/> side.
        /// </summary>
        /// <returns>the opposite side if the given side is <see cref="TeamColour.Red"/> or <see cref="TeamColour.Blue"/>,
        /// else the <paramref name="colour"/> itself.
        /// </returns>
        public static TeamColour GetOppositeSide(this TeamColour colour) => colour switch
        {
            TeamColour.Red => TeamColour.Blue,
            TeamColour.Blue => TeamColour.Red,
            _ => colour,
        };

        /// <summary>
        /// Get the corresponding colour of a team.
        /// </summary>
        /// <param name="teamColour">the <see cref="TeamColour"/> of the specific team.</param>
        /// <param name="fallback">the alternative <see cref="ColourInfo"/> to use
        /// when <paramref name="teamColour"/> is not red nor blue.</param>
        /// <returns>a <see cref="ColourInfo"/> representing the target colour.</returns>
        public static ColourInfo GetTeamColour(TeamColour? teamColour, ColourInfo? fallback = null)
            => teamColour switch
            {
                TeamColour.Red => COLOUR_RED,
                TeamColour.Blue => COLOUR_BLUE,
                _ => fallback ?? COLOUR_NEUTRAL,
            };

        public static ColourInfo GetTypeColour(ChoiceType? type, ColourInfo? fallback = null)
            => type switch
            {
                ChoiceType.Pick => new OsuColour().Green,
                ChoiceType.RedWin => COLOUR_RED,
                ChoiceType.BlueWin => COLOUR_BLUE,
                ChoiceType.Consumed => Color4.Gray,
                _ => fallback ?? COLOUR_CHOICES,
            };

        /// <summary>
        /// Get the representation of a team in the form of a <see cref="LocalisableString"/>.
        /// </summary>
        /// <param name="teamColour">the <see cref="TeamColour"/> of the specific team.</param>
        /// <param name="shortForm">whether to return a shortened string, useful in special cases.</param>
        /// <param name="fallback">the alternative <see cref="LocalisableString"/> to use
        /// when <paramref name="teamColour"/> is not red nor blue.</param>
        /// <returns>a <see cref="LocalisableString"/> representing the team.</returns>
        public static LocalisableString GetTeamString(TeamColour? teamColour, bool shortForm = false,
                                                      LocalisableString? fallback = null) =>
            teamColour switch
            {
                TeamColour.Red => shortForm ? BaseStrings.TeamRedShort : BaseStrings.TeamRed,
                TeamColour.Blue => shortForm ? BaseStrings.TeamBlueShort : BaseStrings.TeamBlue,
                _ => fallback ?? (shortForm ? @"?" : BaseStrings.Unknown),
            };

        /// <summary>
        /// Get the corresponding icon based on the acronym of a mod.
        /// </summary>
        /// <param name="modAcronym">the acronym of the mod.</param>
        /// <returns>a <see cref="IconUsage"/> representing the icon.</returns>
        public static IconUsage GetModIcon(string modAcronym) =>
            modAcronym.ToUpperInvariant() switch
            {
                @"NM" => FumoIcon.NoMod,
                @"HR" => FumoIcon.HardRock,
                @"FM" => FumoIcon.FreeMod,
                @"HD" => OsuIcon.ModHidden,
                @"DT" => OsuIcon.ModDoubleTime,
                _ => FontAwesome.Regular.Circle,
            };

        /// <summary>
        /// Get the corresponding <see cref="ChoiceType"/> based on the <see cref="RoundStep"/>
        /// and <see cref="TeamColour"/> information.
        /// </summary>
        /// <param name="step">the current <see cref="RoundStep"/>.</param>
        /// <param name="colour">the current <see cref="TeamColour"/>.</param>
        /// <returns>a <see cref="ChoiceType"/> converted from the input.</returns>
        /// <exception cref="ArgumentException">thrown when attempting to convert a win state without a valid team colour.</exception>
        public static ChoiceType ToChoiceType(RoundStep? step, TeamColour? colour) =>
            step switch
            {
                RoundStep.Ban => ChoiceType.Ban,
                RoundStep.Pick => ChoiceType.Pick,
                RoundStep.Win or RoundStep.Shiro or RoundStep.UpdateOwner => colour switch
                {
                    TeamColour.Red => ChoiceType.RedWin,
                    TeamColour.Blue => ChoiceType.BlueWin,
                    _ => throw new ArgumentException(@$"The {nameof(RoundStep.Win)} and {nameof(RoundStep.Shiro)} cannot be converted to {nameof(ChoiceType)} without a valid {nameof(TeamColour)}."),
                },
                _ => ChoiceType.Neutral,
            };

        /// <summary>
        /// Turn a mod acronym into the form of <see cref="LegacyMods"/>.
        /// </summary>
        /// <param name="acronym">the acronym of a mod</param>
        /// <returns>the corresponding <see cref="LegacyMods"/>. When not found, returns None.</returns>
        public static LegacyMods ToModEnum(string acronym) =>
            acronym.ToUpperInvariant() switch
            {
                @"HR" => LegacyMods.HardRock,
                @"HD" => LegacyMods.Hidden,
                @"DT" => LegacyMods.DoubleTime,
                _ => LegacyMods.None,
            };
    }
}
