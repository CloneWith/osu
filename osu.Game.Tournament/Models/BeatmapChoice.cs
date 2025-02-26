// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A beatmap choice by a team from a tournament's map pool.
    /// </summary>
    [Serializable]
    public class BeatmapChoice
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour Team = TeamColour.Neutral;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ChoiceType Type = ChoiceType.Neutral;

        public int BeatmapID;

        public BeatmapChoice()
        {
        }

        public BeatmapChoice(int beatmapID)
        {
            BeatmapID = beatmapID;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TeamColour
    {
        /// <summary>
        /// The default colour. Used for winner, EX detection, etc.
        /// </summary>
        None,
        Red,
        Blue,

        /// <summary>
        /// Internally given colour. Should only be used by actions.
        /// </summary>
        Neutral
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChoiceType
    {
        /// <summary>
        /// Another special type placeholder, shouldn't be used in normal conditions.
        /// </summary>
        Neutral,
        Pick,
        Ban,
        RedWin,
        BlueWin,
    }
}
