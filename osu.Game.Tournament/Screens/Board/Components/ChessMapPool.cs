// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class ChessMapPool : Container
    {
        public IEnumerable<FumoBeatmapPanel> MapPanels => sectionFlow.Children.SelectMany(s => s.Cards);

        private FillFlowContainer<ModMapSection> sectionFlow = null!;

        public ChessMapPool()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Name = @"Background box",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new GridContainer
                {
                    Name = @"Content grid",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    RowDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    ColumnDimensions =
                    [
                        new Dimension(),
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = ScreenStrings.MapPool,
                                Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                Margin = new MarginPadding { Vertical = 5 },
                            },
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                Padding = new MarginPadding { Vertical = 5 },
                                Child = sectionFlow = new FillFlowContainer<ModMapSection>
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(15),
                                    ChildrenEnumerable = TournamentGame.MODS.Select(kv => new ModMapSection(kv.Key, kv.Value)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 1,
                                    }),
                                },
                            },
                        },
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(2),
                                Margin = new MarginPadding { Vertical = 5 },
                                Children = new Drawable[]
                                {
                                    new TournamentSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = BoardStrings.RemainingHeader,
                                        Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(5),
                                        ChildrenEnumerable = TournamentGame.MODS.Select(kv => new ModChessCounterText(kv.Key)),
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }
    }
}
