// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseContainer : CompositeDrawable
    {
        public OsuScreenStack ScreenStack { get; private set; }

        public readonly BeatmapAttributesDisplay BeatmapAttributes;
        public readonly ShowcaseBeatmapInfoArea BeatmapInfoDisplay;

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
                BeatmapAttributes = new BeatmapAttributesDisplay
                {
                    RelativePositionAxes = Axes.Both,
                    Alpha = 0,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -0.01f,
                    Y = -0.1f,
                    Mods = { Value = new List<Mod>() },
                    Collapsed = { Value = false }
                },
                BeatmapInfoDisplay = new ShowcaseBeatmapInfoArea
                {
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.16f,
                    Alpha = 0,
                    X = 0.01f,
                    Y = 0.2f
                }
            };
        }

        private partial class PlayerContainer : Container
        {
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;
        }
    }
}
