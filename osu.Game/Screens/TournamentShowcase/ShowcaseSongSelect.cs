// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseSongSelect : SongSelect
    {
        // The footer is unused, thus hiding it.
        protected override bool ShowSongSelectFooter => false;

        public override bool AllowEditing => false;

        [Resolved(canBeNull: true)]
        private IDialogOverlay? dialogOverlay { get; set; }

        private readonly Bindable<BeatmapInfo> targetBeatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<ScoreInfo?> targetScore = new Bindable<ScoreInfo?>();

        public ShowcaseSongSelect(Bindable<BeatmapInfo> beatmap, Bindable<ScoreInfo?> score)
        {
            targetBeatmap.BindTo(beatmap);
            targetScore.BindTo(score);
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

            this.Exit();
            return true;
        }

        private void scoreSelected(ScoreInfo s)
        {
            if (s.BeatmapInfo != null)
            {
                if (s.BeatmapInfo.ID != targetBeatmap.Value.ID)
                {
                    dialogOverlay?.Push(new ProfileCheckFailedDialog
                    {
                        HeaderText = @"Inconsistency Detected",
                        BodyText = @"Please select a score that matches the selected beatmap."
                    });
                    return;
                }

                if (!s.Passed)
                {
                    dialogOverlay?.Push(new ProfileCheckFailedDialog
                    {
                        HeaderText = @"Failed Score",
                        BodyText = @"Use a passed score to guarantee the showcase running properly."
                    });
                    return;
                }

                targetScore.Value = s;
            }
        }
    }
}
