// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseSongSelect : SongSelect, ISongSelect, ISubScreenWithTitle
    {
        public string ShortTitle => @"Item Select";

        public LocalisableString LocalisableTitle => TournamentShowcaseStrings.ItemSelect;

        public event Action<SelectResult>? OnSelect;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        private readonly Bindable<BeatmapInfo> targetBeatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<ScoreInfo?> targetScore = new Bindable<ScoreInfo?>();
        private readonly BindableList<Mod> targetMods = new BindableList<Mod>();
        private readonly Bindable<RulesetInfo> targetRuleset = new Bindable<RulesetInfo>();

        private SelectResult status = SelectResult.None;

        public ShowcaseSongSelect(Bindable<BeatmapInfo> beatmap, BindableList<Mod> mods,
                                  Bindable<ScoreInfo?> score, Bindable<RulesetInfo> rulesetInfo)
        {
            targetBeatmap.BindTo(beatmap);
            targetScore.BindTo(score);
            targetMods.BindTo(mods);
            targetRuleset.BindTo(rulesetInfo);

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
            TopPadding = Header.HEIGHT - 10;
        }

        protected override void OnStart()
        {
            applyCommonInfo();

            // We should clear the score to null, which also means an update,
            targetScore.Value = null;
            status |= SelectResult.ScoreUpdated;

            OnSelect?.Invoke(status);
            this.Exit();
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => [];

        void ISongSelect.PresentScore(ScoreInfo score, ScorePresentType presentType)
        {
            if (score.BeatmapInfo == null)
                return;

            // The "Passed" property may not reflect the actual state.
            if (!score.Passed || score.Rank is ScoreRank.F)
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Text = "This is a failed score. Use a passed score to guarantee the showcase running properly.",
                });

                return;
            }

            applyCommonInfo();
            status |= SelectResult.ScoreUpdated;
            targetScore.Value = score;
            targetRuleset.Value = score.Ruleset;

            OnSelect?.Invoke(status);
            this.Exit();
        }

        private void applyCommonInfo()
        {
            // Pass information of the selected beatmap to the bindable.
            if (targetBeatmap.Value.Hash != Beatmap.Value.BeatmapInfo.Hash)
                status |= SelectResult.BeatmapUpdated;

            if (targetRuleset.Value.OnlineID != Ruleset.Value.OnlineID)
                status |= SelectResult.RulesetUpdated;

            if (!targetMods.SequenceEqual(Mods.Value))
                status |= SelectResult.ModUpdated;

            targetBeatmap.Value = Beatmap.Value.BeatmapInfo;
            targetRuleset.Value = Ruleset.Value;
            targetMods.Clear();
            targetMods.AddRange(Mods.Value);
        }
    }

    [Flags]
    public enum SelectResult
    {
        /// <summary>
        /// No change was made to the original selection.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only the selected mod has been updated.
        /// This should only happen when no score was selected.
        /// </summary>
        ModUpdated = 2 << 1,

        /// <summary>
        /// The score has been updated.
        /// </summary>
        ScoreUpdated = 2 << 2,

        /// <summary>
        /// The ruleset has been changed.
        /// </summary>
        RulesetUpdated = 2 << 3,

        /// <summary>
        /// The beatmap has been reselected.
        /// </summary>
        BeatmapUpdated = 2 << 4,
    }
}
