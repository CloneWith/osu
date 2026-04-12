// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class SimpleShowcaseControl : GridContainer
    {
        public SimpleShowcaseControl(Action exitAction)
        {
            RelativeSizeAxes = Axes.X;
            Height = 32;
            Padding = new MarginPadding { Horizontal = 10 };

            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Relative),
                new Dimension(),
            };

            Content = new[]
            {
                new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(4),
                        Children = new Drawable[]
                        {
                            new ClickTwiceButton
                            {
                                IdleIcon = OsuIcon.Cross,
                                IdleText = CommonStrings.Exit,
                                ActiveText = TournamentShowcaseStrings.ExitConfirmText,
                                Action = exitAction,
                            },
                            new StateSwitchButton
                            {
                                IdleIcon = OsuIcon.Debug,
                                IdleText = TournamentShowcaseStrings.ManualControlState,
                                ActiveText = TournamentShowcaseStrings.AutoControlState,
                            },
                        },
                    },
                },
            };
        }
    }
}
