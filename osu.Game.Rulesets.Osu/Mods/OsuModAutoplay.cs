﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IO.Archives;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAutoplay : ModAutoplay, IApplicableToHUD, IApplicableToPlayer
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModAutopilot)).Append(typeof(OsuModSpunOut)).ToArray();

        [SettingSource("Cursor Dance", "Enable cursor dance")]
        public Bindable<bool> CursorDance { get; } = new BindableBool();

        [SettingSource("Save score")]
        public Bindable<bool> SaveScore { get; } = new BindableBool();

        [SettingSource("Hide replay interface")]
        public Bindable<bool> HideInterface { get; } = new BindableBool();

        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
            return new Score
            {
                ScoreInfo = new ScoreInfo
                {
                    User = new User { Username = CursorDance.Value ? "danser" : "Autoplay" },
                    Beatmap = beatmap.BeatmapInfo,
                    BeatmapInfoID = beatmap.BeatmapInfo.Metadata.ID,
                    Date = DateTime.UtcNow,
                    Mods = mods.ToArray()
                },
                Replay = CursorDance.Value ? new OsuDanceGenerator(beatmap, mods).Generate() : new OsuAutoGenerator(beatmap, mods).Generate()
            };
        }

        public void ApplyToHUD(HUDOverlay overlay)
        {
            if (HideInterface.Value)
                overlay.PlayerSettingsOverlay.Children.ForEach(child => child.Hide());
        }

        public void ApplyToPlayer(Player player) => ((ReplayPlayer)player).SaveScore = SaveScore.Value;
    }
}
