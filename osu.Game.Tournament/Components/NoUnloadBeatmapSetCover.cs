// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Tournament.Components
{
    public partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
    {
        private readonly BeatmapSetCoverType coverType;

        public NoUnloadBeatmapSetCover(BeatmapSetCoverType coverType = BeatmapSetCoverType.Cover)
            : base(coverType)
        {
            this.coverType = coverType;
        }

        // As covers are displayed on stream, we want them to load as soon as possible.
        protected override double LoadDelay => 0;

        // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);

        protected override Drawable CreateDrawable(IBeatmapSetOnlineInfo model)
        {
            if (model == null)
                return null;

            return new TournamentBeatmapSetCover(model, coverType)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            };
        }
    }
}
