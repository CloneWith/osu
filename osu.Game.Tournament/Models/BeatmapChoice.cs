﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        public TeamColour Team;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ChoiceType Type;

        public int BeatmapID;

        // For auto selecting check.
        public bool Token = false;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TeamColour
    {
        /// <summary>
        /// Used for winner, EX detection and as the default colour.
        /// </summary>
        None,
        Red,
        Blue,
        /// <summary>
        /// Should only be used by actions.
        /// </summary>
        Neutral
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChoiceType
    {
        /// <summary>
        /// Another special type placeholder, shouldn't be used in normal conditions
        /// </summary>
        Neutral,
        Pick,
        Ban,
        Protect,
        RedWin,
        BlueWin,
        Trap,
        /// <summary>
        /// A special type, Swap specific
        /// </summary>
        Swap
    }
}
