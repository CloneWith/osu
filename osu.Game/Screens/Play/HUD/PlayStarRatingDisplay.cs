// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayStarRatingDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private IBindable<StarDifficulty?>? difficultyBindable;
        private CancellationTokenSource? difficultyCancellationSource;
        private StarRatingDisplay display = null!;

        public PlayStarRatingDisplay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            difficultyBindable = difficultyCache.GetBindableDifficulty(beatmap.Value.BeatmapInfo);
            InternalChild = display = new StarRatingDisplay(difficultyBindable.Value ?? new StarDifficulty());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b =>
            {
                difficultyCancellationSource?.Cancel();
                difficultyCancellationSource = new CancellationTokenSource();

                difficultyBindable?.UnbindAll();
                difficultyBindable = difficultyCache.GetBindableDifficulty(b.NewValue.BeatmapInfo, difficultyCancellationSource.Token);
                difficultyBindable.BindValueChanged(d =>
                {
                    display.Current.Value = d.NewValue ?? new StarDifficulty();
                });
            }, true);
        }
    }
}
