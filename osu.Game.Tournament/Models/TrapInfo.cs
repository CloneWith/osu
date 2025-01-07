﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK.Graphics;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds all data about traps for our tournament.
    /// </summary>
    [Serializable]
    public class TrapInfo
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour Team;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TrapType Mode;

        public int BeatmapID;

        [JsonIgnore]
        public SpriteIcon Icon { get; private set; } = new SpriteIcon
        {
            Icon = FontAwesome.Solid.QuestionCircle,
            Colour = Color4.White,
        };

        [JsonIgnore]
        public string Name { get; private set; }

        [JsonIgnore]
        public string Description { get; private set; }

        /// <summary>
        /// A constructor to set up an instance of <see cref="TrapInfo"/>.
        /// </summary>
        /// <param name="colour">The team which set the trap (if exists).</param>
        /// <param name="type">The trap type (see <see cref="TrapType"/> for available values).</param>
        /// <param name="mapID">The beatmap ID which the trap is set on.</param>
        public TrapInfo(TeamColour colour = TeamColour.Neutral, TrapType type = TrapType.Unknown, int mapID = 0)
        {
            Team = colour;
            Mode = type;
            BeatmapID = mapID;

            switch (type)
            {
                case TrapType.Swap:
                    Name = @"大陆漂移";
                    Description = @"游玩结束后，此格子将与另一个交换";
                    Icon.Icon = FontAwesome.Solid.ExchangeAlt;
                    Icon.Colour = Color4.Orange;
                    break;

                case TrapType.Reverse:
                    Name = @"时空之门";
                    Description = @"以对方胜利进行结算，此后回到原状态";
                    Icon.Icon = FontAwesome.Solid.Clock;
                    Icon.Colour = Color4.SkyBlue;
                    break;

                case TrapType.Unused:
                    Name = @"陷阱无效";
                    Description = @"布置方触发了陷阱，将不会生效";
                    Icon.Icon = FontAwesome.Solid.Check;
                    Icon.Colour = Color4.White;
                    break;

                default:
                    Name = @"Unknown Trap";
                    Description = @"We don't know this one.";
                    break;
            }
        }

        /// <summary>
        /// Get the original trap type based on a string.
        /// The string should exactly match the name of the trap.
        /// </summary>
        /// <param name="typeString">A <see cref="LocalisableString"/>, the string to handle</param>
        /// <returns>A <see cref="TrapType"/>, representing the trap type</returns>
        public TrapType GetReversedType(LocalisableString typeString)
        {
            switch (typeString.ToString())
            {
                case @"时空之门":
                    return TrapType.Reverse;

                case @"大陆漂移":
                    return TrapType.Swap;

                case @"陷阱无效":
                    return TrapType.Unused;

                default:
                    return TrapType.Unknown;
            }
        }
    }

    /// <summary>
    /// Lists out all available trap types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrapType
    {
        /// <summary>
        /// Swap the specified map block with another after the current gameplay comes to an end.
        /// </summary>
        [Description("大陆漂移")]
        Swap,

        /// <summary>
        /// Set a temporary Win state for this block, then reset to Pick.
        /// </summary>
        [Description("时空之门")]
        Reverse,

        /// <summary>
        /// The trap has no effect.
        /// </summary>
        Unused,

        /// <summary>
        /// Placeholder for unimplemented or empty traps.
        /// </summary>
        Unknown,
    }
}
