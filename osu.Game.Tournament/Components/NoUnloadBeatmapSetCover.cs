// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Tournament.Components
{
    public partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
    {
        // As covers are displayed on stream, we want them to load as soon as possible.
        protected override double LoadDelay => 0;

        // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);
    }
}
