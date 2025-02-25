// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
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

        private SpriteIcon icon = new SpriteIcon
        {
            Icon = FontAwesome.Regular.GrinWink,
            Colour = new OsuColour().Yellow,
        };

        private LocalisableString teamPrompt;

        /// <summary>
        /// A constructor to set up an instance of <see cref="InstructionInfo"/>.
        /// </summary>
        /// <param name="team">The current team.</param>
        /// <param name="roundStep">The current step.</param>
        public InstructionInfo(TeamColour team = TeamColour.Neutral, RoundStep roundStep = RoundStep.Default)
        {
            RoundStep = roundStep;

            bool notDraw = team == TeamColour.Red || team == TeamColour.Blue;

            teamPrompt = team == TeamColour.Red ? @"红队" : (team == TeamColour.Blue ? @"蓝队" : string.Empty);

            switch (RoundStep)
            {
                case RoundStep.Ban:
                    Name = @$"标记禁图·{teamPrompt}";
                    Description = @"被禁止的图无法被选与设置陷阱。";
                    icon.Icon = FontAwesome.Solid.Ban;
                    icon.Colour = Color4.Orange;
                    break;

                case RoundStep.Pick:
                    Name = @$"标记选图·{teamPrompt}";
                    Description = @"选择该轮要游玩的图。";
                    icon.Icon = FontAwesome.Solid.Check;
                    icon.Colour = new OsuColour().Green;
                    break;

                case RoundStep.Win:
                    Name = @$"胜方染色·{teamPrompt}";
                    Description = @"此图所在格将染成获胜队颜色。";
                    icon.Icon = FontAwesome.Solid.Trophy;
                    icon.Colour = team == TeamColour.Red ? new OsuColour().Pink : (team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow);
                    break;

                case RoundStep.TieBreaker:
                    Name = @"即将进入 EX 模式";
                    Description = @"当前棋盘不足以任一方取胜，需要重新染色。";
                    icon.Icon = FontAwesome.Solid.Bolt;
                    icon.Colour = Color4.Orange;
                    break;

                case RoundStep.FinalWin:
                    Name = notDraw ? @$"{teamPrompt}获胜！" : team == TeamColour.Neutral ? @"EX: 决胜局" : @"Do you want smoke?";
                    Description = notDraw ? @$"恭喜{teamPrompt}获得最终胜利！" : team == TeamColour.Neutral ? @"我只是个笨蛋，也没有你聪明。" : @"来看看礼堂顶针？";
                    icon.Icon = notDraw ? FontAwesome.Solid.Medal : FontAwesome.Solid.Asterisk;
                    icon.Colour = team == TeamColour.Red ? new OsuColour().Pink : (team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow);
                    break;

                case RoundStep.Halt:
                    Name = @"请稍候...";
                    Description = @"等待裁判响应...";
                    icon.Icon = FontAwesome.Solid.ExclamationCircle;
                    icon.Colour = Color4.Orange;
                    break;

                default:
                    Name = @"Welcome to the Fumo era!";
                    Description = @"(ᗜˬᗜ)";
                    break;
            }
        }

        public LocalisableString Name { get; }

        public LocalisableString Description { get; }

        public IconUsage Icon => icon.Icon;
        public ColourInfo IconColor => icon.Colour;

        /// <summary>
        /// Get the original step type based on a string.
        /// The string should exactly match the name of the step.
        /// </summary>
        /// <param name="typeString">A <see cref="LocalisableString"/>, the string to handle</param>
        /// <returns>A <see cref="RoundStep"/>, representing the current step</returns>
        public RoundStep GetReversedType(LocalisableString typeString)
        {
            switch (typeString.ToString())
            {
                case @"标记禁图":
                    return RoundStep.Ban;

                case @"标记选图":
                    return RoundStep.Pick;

                case @"胜方染色":
                    return RoundStep.Win;

                case @"最后一战":
                    return RoundStep.TieBreaker;

                case @"请稍候...":
                    return RoundStep.Halt;

                default:
                    return RoundStep.Default;
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
