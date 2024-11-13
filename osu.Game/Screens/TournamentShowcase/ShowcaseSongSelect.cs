// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
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

        private PlayBeatmapDetailArea playBeatmapDetailArea = null!;

        private readonly Bindable<BeatmapInfo> targetBeatmap = new Bindable<BeatmapInfo>();

        public ShowcaseSongSelect(Bindable<BeatmapInfo> beatmap)
        {
            targetBeatmap.BindTo(beatmap);
        }

        protected void PresentScore(ScoreInfo score) =>
            FinaliseSelection(score.BeatmapInfo, score.Ruleset, () => this.Push(new SoloResultsScreen(score)));

        protected override BeatmapDetailArea CreateBeatmapDetailArea()
        {
            playBeatmapDetailArea = new PlayBeatmapDetailArea
            {
                Leaderboard =
                {
                    ScoreSelected = PresentScore
                }
            };

            return playBeatmapDetailArea;
        }

        protected override bool OnStart()
        {
            // Pass information of the selected beatmap to the bindable.
            targetBeatmap.Value = Beatmap.Value.BeatmapInfo;

            this.Exit();
            return true;
        }
    }
}
