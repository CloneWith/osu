// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;

namespace osu.Game.Tournament.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackgroundType
    {
        [LocalisableDescription(typeof(BackgroundSelectStrings), nameof(BackgroundSelectStrings.DefaultBackground))]
        Main,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Bracket))]
        Ladder,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Schedule))]
        Schedule,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Drawings))]
        Drawings,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Showcase))]
        Showcase,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Seeding))]
        Seeding,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.TeamIntro))]
        TeamIntro,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Gameplay))]
        Gameplay,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.MapPool))]
        MapPool,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.RedWin))]
        RedWin,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.BlueWin))]
        BlueWin,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.DrawWin))]
        Draw,

        [LocalisableDescription(typeof(ScreenStrings), nameof(ScreenStrings.Board))]
        Board,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackgroundSource
    {
        /// <summary>
        /// Reserved zero position for auto-detection.
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
            => Source == other.Source && Name == other.Name && Dim == other.Dim;

        public bool FileInfoEquals(BackgroundInfo other)
            => Source == other.Source && Name == other.Name;

        public override bool Equals(object? obj)
            => obj is BackgroundInfo other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine((int)Source, Name);

        public static bool operator ==(BackgroundInfo left, BackgroundInfo right)
            => left.Equals(right);

        public static bool operator !=(BackgroundInfo left, BackgroundInfo right)
            => !(left == right);

        #endregion
    }

    public static class BackgroundProps
    {
        /// <summary>
        /// The default mapping of screen backgrounds.
        /// </summary>
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

        /// <summary>
        /// Gets the default background mapping for a background type.
        /// </summary>
        public static BackgroundInfo GetDefaultBackgroundInfo(BackgroundType backgroundType)
            => PATHS.GetBackgroundInfo(backgroundType);
    }

    public static class BackgroundMapExtensions
    {
        /// <summary>
        /// Tries to find the most recent mapping for a background type.
        /// </summary>
        public static bool TryGetBackgroundInfo(this BindableList<KeyValuePair<BackgroundType, BackgroundInfo>> backgroundMap,
                                                BackgroundType backgroundType, out BackgroundInfo backgroundInfo)
        {
            for (int i = backgroundMap.Count - 1; i >= 0; i--)
            {
                if (backgroundMap[i].Key != backgroundType)
                    continue;

                backgroundInfo = backgroundMap[i].Value;
                return true;
            }

            backgroundInfo = default;
            return false;
        }

        /// <summary>
        /// Gets the mapping for a background type, falling back to the default mapping if needed.
        /// </summary>
        public static BackgroundInfo GetBackgroundInfo(this BindableList<KeyValuePair<BackgroundType, BackgroundInfo>> backgroundMap, BackgroundType backgroundType)
            => backgroundMap.TryGetBackgroundInfo(backgroundType, out var backgroundInfo)
                ? backgroundInfo
                : BackgroundProps.GetDefaultBackgroundInfo(backgroundType);

        /// <summary>
        /// Updates the mapping for a background type while keeping only one effective entry.
        /// </summary>
        public static void SetBackgroundInfo(this BindableList<KeyValuePair<BackgroundType, BackgroundInfo>> backgroundMap, BackgroundType backgroundType, BackgroundInfo backgroundInfo)
        {
            bool updated = false;

            for (int i = backgroundMap.Count - 1; i >= 0; i--)
            {
                if (backgroundMap[i].Key != backgroundType)
                    continue;

                if (!updated)
                {
                    backgroundMap[i] = new KeyValuePair<BackgroundType, BackgroundInfo>(backgroundType, backgroundInfo);
                    updated = true;
                }
                else
                {
                    backgroundMap.RemoveAt(i);
                }
            }

            if (!updated)
                backgroundMap.Add(new KeyValuePair<BackgroundType, BackgroundInfo>(backgroundType, backgroundInfo));
        }
    }
}
