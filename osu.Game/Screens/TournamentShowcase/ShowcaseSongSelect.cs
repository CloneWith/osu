// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseSongSelect : SongSelect
    {
        public override bool AllowEditing => false;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        private readonly Bindable<BeatmapInfo> targetBeatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<ScoreInfo?> targetScore = new Bindable<ScoreInfo?>();
        private readonly BindableList<Mod> targetMods = new BindableList<Mod>();
        private readonly Bindable<RulesetInfo> targetRuleset = new Bindable<RulesetInfo>();

        public ShowcaseSongSelect(Bindable<BeatmapInfo> beatmap, BindableList<Mod> mods,
                                  Bindable<ScoreInfo?> score, Bindable<RulesetInfo> rulesetInfo)
        {
            targetBeatmap.BindTo(beatmap);
            targetScore.BindTo(score);
            targetMods.BindTo(mods);
            targetRuleset.BindTo(rulesetInfo);
        }

        protected void PresentScore(ScoreInfo score) =>
            FinaliseSelection(score.BeatmapInfo, score.Ruleset, () => this.Push(new SoloResultsScreen(score)));

        protected override BeatmapDetailArea CreateBeatmapDetailArea()
        {
            return new ShowcaseBeatmapDetailArea
            {
                Leaderboard =
                {
                    ScoreSelected = scoreSelected
                }
            };
        }

        protected override bool OnStart()
        {
            // Pass information of the selected beatmap to the bindable.
            targetBeatmap.Value = Beatmap.Value.BeatmapInfo;
            targetRuleset.Value = Ruleset.Value;
            targetMods.Clear();
            targetMods.AddRange(Mods.Value);

            this.Exit();
            return true;
        }

        private void scoreSelected(ScoreInfo s)
        {
            if (s.BeatmapInfo != null)
            {
                if (!s.Passed)
                {
                    dialogOverlay?.Push(new ProfileCheckFailedDialog
                    {
                        HeaderText = @"Failed Score",
                        BodyText = @"Use a passed score to guarantee the showcase running properly."
                    });
                    return;
                }

                targetBeatmap.Value = Beatmap.Value.BeatmapInfo;
                targetScore.Value = s;
                targetRuleset.Value = s.Ruleset;
                targetMods.Clear();
                targetMods.AddRange(s.Mods);

                this.Exit();
            }
        }
    }
}
