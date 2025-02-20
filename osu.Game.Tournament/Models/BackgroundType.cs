// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackgroundType
    {
        [Description("Default Background")]
        Main,

        [Description("Bracket Screen")]
        Ladder,

        [Description("Schedule Screen")]
        Schedule,

        [Description("Drawings Screen")]
        Drawings,

        [Description("Showcase Screen")]
        Showcase,

        [Description("Seeding Screen")]
        Seeding,

        [Description("Team Introduction")]
        TeamIntro,

        [Description("Gameplay Screen")]
        Gameplay,

        [Description("Map Pool Screen")]
        MapPool,

        [Description("Win Screen (Red)")]
        RedWin,

        [Description("Win Screen (Blue)")]
        BlueWin,

        [Description("Win Screen (Draw)")]
        Draw,

        [Description("Board Screen")]
        Board,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackgroundSource
    {
        /// <summary>
        /// Reserved zero position for auto detection.
        /// </summary>
        Auto,
        Video,
        Image,
    }

    [Serializable]
    public struct BackgroundInfo : IEquatable<BackgroundInfo>
    {
        public BackgroundSource Source;
        public string Name;
        public float Dim;

        #region Constructors

        public BackgroundInfo()
        {
            Source = BackgroundSource.Video;
            Name = string.Empty;
        }

        public BackgroundInfo(BackgroundSource source, string name, float dim = 0)
        {
            Source = source;
            Name = name;
            Dim = dim;
        }

        public BackgroundInfo(string name)
        {
            Source = BackgroundSource.Video;
            Name = name;
        }

        #endregion

        #region Operators

        public bool Equals(BackgroundInfo other)
        {
            return Source == other.Source
                   && Name == other.Name
                   && Dim == other.Dim;
        }

        public bool FileInfoEquals(BackgroundInfo other)
        {
            return Source == other.Source
                   && Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is BackgroundInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Source, Name);
        }

        #endregion
    }

    public static class BackgroundProps
    {
        public static readonly BindableList<KeyValuePair<BackgroundType, BackgroundInfo>> PATHS = new BindableList<KeyValuePair<BackgroundType, BackgroundInfo>>
        {
            KeyValuePair.Create(BackgroundType.Gameplay, new BackgroundInfo("gameplay")),
            KeyValuePair.Create(BackgroundType.MapPool, new BackgroundInfo("mappool")),
            KeyValuePair.Create(BackgroundType.Main, new BackgroundInfo("main")),
            KeyValuePair.Create(BackgroundType.Ladder, new BackgroundInfo("ladder")),
            KeyValuePair.Create(BackgroundType.Schedule, new BackgroundInfo("schedule")),
            KeyValuePair.Create(BackgroundType.Drawings, new BackgroundInfo("drawings")),
            KeyValuePair.Create(BackgroundType.Showcase, new BackgroundInfo("showcase")),
            KeyValuePair.Create(BackgroundType.Seeding, new BackgroundInfo("seeding")),
            KeyValuePair.Create(BackgroundType.TeamIntro, new BackgroundInfo("teamintro")),
            KeyValuePair.Create(BackgroundType.RedWin, new BackgroundInfo("teamwin-red")),
            KeyValuePair.Create(BackgroundType.BlueWin, new BackgroundInfo("teamwin-blue")),
            KeyValuePair.Create(BackgroundType.Draw, new BackgroundInfo("mappool")),
            KeyValuePair.Create(BackgroundType.Board, new BackgroundInfo("mappool")),
        };
    }
}
