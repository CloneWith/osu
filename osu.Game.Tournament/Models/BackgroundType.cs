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
        Video,
        Image,
        None
    }

    [Serializable]
    public struct BackgroundInfo
    {
        public BackgroundSource Source;
        public string Name;
    }

    public static class BackgroundProps
    {
        public static readonly BindableList<KeyValuePair<BackgroundType, BackgroundInfo>> PATHS = new BindableList<KeyValuePair<BackgroundType, BackgroundInfo>>
        {
            KeyValuePair.Create(BackgroundType.Gameplay, new BackgroundInfo { Name = "gameplay" }),
            KeyValuePair.Create(BackgroundType.MapPool, new BackgroundInfo { Name = "mappool" }),
            KeyValuePair.Create(BackgroundType.Main, new BackgroundInfo { Name = "main" }),
            KeyValuePair.Create(BackgroundType.Ladder, new BackgroundInfo { Name = "ladder" }),
            KeyValuePair.Create(BackgroundType.Schedule, new BackgroundInfo { Name = "schedule" }),
            KeyValuePair.Create(BackgroundType.Drawings, new BackgroundInfo { Name = "drawings" }),
            KeyValuePair.Create(BackgroundType.Showcase, new BackgroundInfo { Name = "showcase" }),
            KeyValuePair.Create(BackgroundType.Seeding, new BackgroundInfo { Name = "seeding" }),
            KeyValuePair.Create(BackgroundType.TeamIntro, new BackgroundInfo { Name = "teamintro" }),
            KeyValuePair.Create(BackgroundType.RedWin, new BackgroundInfo { Name = "teamwin-red" }),
            KeyValuePair.Create(BackgroundType.BlueWin, new BackgroundInfo { Name = "teamwin-blue" }),
            KeyValuePair.Create(BackgroundType.Draw, new BackgroundInfo { Name = "mappool" }),
            KeyValuePair.Create(BackgroundType.Board, new BackgroundInfo { Name = "mappool" }),
        };
    }
}
