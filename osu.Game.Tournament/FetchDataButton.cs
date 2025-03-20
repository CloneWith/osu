// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Localisation;

namespace osu.Game.Tournament
{
    internal partial class FetchDataButton : OsuButton
    {
        private readonly Action<bool>? fetchAction;

        public FetchDataButton(Action<bool>? fetchAction = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = 48;
            Text = BaseStrings.FetchData;
            this.fetchAction = fetchAction;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Action = () => fetchAction?.Invoke(true);
        }
    }
}
