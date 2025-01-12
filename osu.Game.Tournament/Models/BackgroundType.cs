// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

    public static class BackgroundProps
    {
        public static readonly BindableList<KeyValuePair<BackgroundType, string>> PATHS = new BindableList<KeyValuePair<BackgroundType, string>>
        {
            KeyValuePair.Create(BackgroundType.Gameplay, "gameplay"),
            KeyValuePair.Create(BackgroundType.MapPool, "mappool"),
            KeyValuePair.Create(BackgroundType.Main, "main"),
            KeyValuePair.Create(BackgroundType.Ladder, "ladder"),
            KeyValuePair.Create(BackgroundType.Schedule, "schedule"),
            KeyValuePair.Create(BackgroundType.Drawings, "drawings"),
            KeyValuePair.Create(BackgroundType.Showcase, "showcase"),
            KeyValuePair.Create(BackgroundType.Seeding, "seeding"),
            KeyValuePair.Create(BackgroundType.TeamIntro, "teamintro"),
            KeyValuePair.Create(BackgroundType.RedWin, "teamwin-red"),
            KeyValuePair.Create(BackgroundType.BlueWin, "teamwin-blue"),
            KeyValuePair.Create(BackgroundType.Draw, "mappool"),
            KeyValuePair.Create(BackgroundType.Board, "mappool"),
        };
    }
}
