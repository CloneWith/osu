// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class WindowSizeIndicator : CompositeDrawable
    {
        private const int entry_spacing = 15;

        private readonly BindableSize sizeBindable;

        private TournamentSpriteText widthText = null!;
        private TournamentSpriteText heightText = null!;

        public WindowSizeIndicator(BindableSize bSize)
        {
            sizeBindable = bSize;
            sizeBindable.BindValueChanged(bindSizeChanged);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 10;
            Alpha = 0;
            AlwaysPresent = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.6f),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(25),
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            RowDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, entry_spacing),
                                new Dimension(GridSizeMode.AutoSize),
                            ],
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, entry_spacing),
                                new Dimension(GridSizeMode.AutoSize),
                            ],
                            Content = new[]
                            {
                                new[]
                                {
                                    new SpriteIcon
                                    {
                                        Icon = FontAwesome.Solid.RulerHorizontal,
                                        Size = new Vector2(24),
                                    },
                                    Empty(),
                                    widthText = new TournamentSpriteText
                                    {
                                        Text = sizeBindable.Value.Width.ToString(),
                                        Colour = TournamentGame.TEXT_COLOUR,
                                        Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                    },
                                },
                                [
                                    Empty(),
                                    Empty(),
                                    Empty(),
                                ],
                                new[]
                                {
                                    new SpriteIcon
                                    {
                                        Icon = FontAwesome.Solid.RulerVertical,
                                        Size = new Vector2(24),
                                    },
                                    Empty(),
                                    heightText = new TournamentSpriteText
                                    {
                                        Text = sizeBindable.Value.Height.ToString(),
                                        Colour = TournamentGame.TEXT_COLOUR,
                                        Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                    },
                                },
                            }
                        },
                    },
                },
            };
        }

        private void bindSizeChanged(ValueChangedEvent<Size> e)
        {
            Scheduler.Add(() =>
            {
                widthText.Text = e.NewValue.Width.ToString();
                heightText.Text = e.NewValue.Height.ToString();
            });
        }
    }
}
