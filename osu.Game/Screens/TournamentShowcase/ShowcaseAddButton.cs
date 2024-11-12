// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseAddButton : OsuAnimatedButton
    {
        private OsuTextFlowContainer textFlow = null!;
        private readonly string label;

        public ShowcaseAddButton(string label, Action? action)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Action = action;
            this.label = label;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(textFlow = new OsuTextFlowContainer(cp => cp.Font = cp.Font.With(size: 20))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding(5)
            });

            textFlow.AddIcon(FontAwesome.Solid.PlusCircle, i =>
            {
                i.Padding = new MarginPadding { Right = 10 };
            });

            textFlow.AddText(label);
        }
    }
}
