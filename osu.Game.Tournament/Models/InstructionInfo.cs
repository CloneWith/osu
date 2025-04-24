// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Tournament.Localisation;
using osuTK.Graphics;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds all data about traps for our tournament.
    /// </summary>
    [Serializable]
    public class InstructionInfo
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour Team;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public RoundStep RoundStep;

        public LocalisableString Name { get; private set; }
        public LocalisableString Description { get; private set; }

        public IconUsage Icon { get; private set; } = FontAwesome.Regular.StickyNote;
        public ColourInfo IconColour { get; private set; } = new OsuColour().Yellow;

        private LocalisableString teamPrompt;
        private LocalisableString shortTeamPrompt;

        /// <summary>
        /// A constructor to set up an instance of <see cref="InstructionInfo"/>.
        /// </summary>
        /// <param name="team">The current team.</param>
        /// <param name="roundStep">The current step.</param>
        public InstructionInfo(TeamColour team = TeamColour.Neutral, RoundStep roundStep = RoundStep.Default)
        {
            Team = team;
            RoundStep = roundStep;

            bool notDraw = team == TeamColour.Red || team == TeamColour.Blue;

            teamPrompt = team switch
            {
                TeamColour.Red => BaseStrings.TeamRed,
                TeamColour.Blue => BaseStrings.TeamBlue,
                _ => BaseStrings.Unknown,
            };

            shortTeamPrompt = team switch
            {
                TeamColour.Red => BaseStrings.TeamRedShort,
                TeamColour.Blue => BaseStrings.TeamBlueShort,
                _ => @"?",
            };

            switch (RoundStep)
            {
                case RoundStep.Ban:
                    Name = InstructionsStrings.BanName(shortTeamPrompt);
                    Description = InstructionsStrings.BanDescription;
                    Icon = FontAwesome.Solid.Ban;
                    IconColour = Color4.Orange;
                    break;

                case RoundStep.Pick:
                    Name = InstructionsStrings.PickName(shortTeamPrompt);
                    Description = InstructionsStrings.PickDescription;
                    Icon = FontAwesome.Solid.Check;
                    IconColour = new OsuColour().Green;
                    break;

                case RoundStep.Win:
                    Name = InstructionsStrings.WinName(shortTeamPrompt);
                    Description = InstructionsStrings.WinDescription;
                    Icon = FontAwesome.Solid.Trophy;
                    IconColour = team == TeamColour.Red ? new OsuColour().Pink : team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow;
                    break;

                case RoundStep.Shiro:
                    Name = InstructionsStrings.ShiroName(shortTeamPrompt);
                    Description = InstructionsStrings.ShiroDescription;
                    Icon = FontAwesome.Regular.Circle;
                    IconColour = team == TeamColour.Red ? new OsuColour().Pink : team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow;
                    break;

                case RoundStep.TieBreaker:
                    Name = InstructionsStrings.TieBreakerName;
                    Description = InstructionsStrings.TieBreakerDescription;
                    Icon = FontAwesome.Solid.Bolt;
                    IconColour = Color4.Orange;
                    break;

                case RoundStep.FinalWin:
                    Name = notDraw ? InstructionsStrings.FinalWinName(teamPrompt) : InstructionsStrings.OnFireName;
                    Description = notDraw ? InstructionsStrings.FinalWinDescription : InstructionsStrings.OnFireDescription;
                    Icon = notDraw ? FontAwesome.Solid.Medal : FontAwesome.Solid.Asterisk;
                    IconColour = team == TeamColour.Red ? new OsuColour().Pink : team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow;
                    break;

                case RoundStep.Halt:
                    Name = InstructionsStrings.HaltName;
                    Description = InstructionsStrings.HaltDescription;
                    Icon = FontAwesome.Solid.ExclamationCircle;
                    IconColour = Color4.Orange;
                    break;

                default:
                    Name = InstructionsStrings.DefaultName;
                    Description = InstructionsStrings.DefaultDescription;
                    break;
            }
        }
    }

    /// <summary>
    /// Lists out all possible stages / steps in a tournament round.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoundStep
    {
        /// <summary>
        /// Mark maps unable to be chosen.
        /// </summary>
        Ban,

        /// <summary>
        /// Pick maps.
        /// </summary>
        Pick,

        /// <summary>
        /// Mark colours.
        /// </summary>
        Win,

        /// <summary>
        /// Place the empty chess.
        /// </summary>
        Shiro,

        /// <summary>
        /// The final stage.
        /// </summary>
        TieBreaker,

        /// <summary>
        /// The winner is decided.
        /// </summary>
        FinalWin,

        /// <summary>
        /// Something went wrong.
        /// </summary>
        Halt,

        /// <summary>
        /// Placeholder for default conditions.
        /// </summary>
        Default,
    }
}
