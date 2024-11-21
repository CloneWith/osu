// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseContainer : CompositeDrawable
    {
        public OsuScreenStack ScreenStack { get; private set; }

        public ShowcaseContainer()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new PlayerContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = ScreenStack = new OsuScreenStack
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
            };
        }

        private partial class PlayerContainer : Container
        {
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;
        }
    }
}
