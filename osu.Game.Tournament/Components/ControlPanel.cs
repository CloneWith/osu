// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Tournament.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// An element anchored to the right-hand area of a screen that provides streamer level controls.
    /// Should be off-screen.
    /// </summary>
    public partial class ControlPanel : Container
    {
        private readonly FillFlowContainer buttons;

        protected override Container<Drawable> Content => buttons;

        public ControlPanel(bool needSaving = false)
        {
            Name = "Control Panel Sidebar";
            RelativeSizeAxes = Axes.Y;
            AlwaysPresent = true;
            Width = TournamentSceneManager.CONTROL_AREA_WIDTH;
            Anchor = Anchor.TopRight;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(54, 54, 54, 255)
                },
                new GridContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Padding = new MarginPadding { Vertical = 10 },
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Icon = OsuIcon.Settings,
                                        Size = new Vector2(20),
                                    },
                                    new TournamentSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = BaseStrings.ControlPanel,
                                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 24),
                                    },
                                }
                            },
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                Child = buttons = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(5),
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5f),
                                },
                            },
                        },
                        new[]
                        {
                            needSaving
                                ? new SaveChangesButton
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Padding = new MarginPadding(5),
                                }
                                : Empty(),
                        },
                    }
                },
            };
        }

        public partial class Spacer : CompositeDrawable
        {
            public Spacer(float height = 20)
            {
                RelativeSizeAxes = Axes.X;
                Height = height;
                AlwaysPresent = true;
            }
        }
    }
}
