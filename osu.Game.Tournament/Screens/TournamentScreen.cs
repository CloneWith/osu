﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    public abstract partial class TournamentScreen : CompositeDrawable
    {
        public const double FADE_DELAY = 200;

        public bool HadBeenSelected { get; protected set; }

        [Resolved]
        public LadderInfo LadderInfo { get; private set; } = null!;

        protected TournamentScreen()
        {
            RelativeSizeAxes = Axes.Both;

            FillMode = FillMode.Fit;
            FillAspectRatio = 16 / 9f;
        }

        /// <summary>
        /// Called when the screen is selected the first time in this session.
        /// </summary>
        /// <param name="enforced">Enforce this function to be executed even the screen had been selected before.</param>
        public virtual void FirstSelected(bool enforced = false)
        {
            if (HadBeenSelected && !enforced) return;

            HadBeenSelected = true;
        }

        public override void Hide() => this.FadeOut(FADE_DELAY);

        public override void Show() => this.FadeIn(FADE_DELAY);
    }
}
