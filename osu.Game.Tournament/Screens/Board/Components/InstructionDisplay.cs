// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    /// <summary>
    /// A rounded display of tournament match steps with a specific icon.
    /// </summary>
    public partial class InstructionDisplay : CompositeDrawable
    {
        public TeamColour Team
        {
            get => thisStep.Team;
            set
            {
                thisStep = new InstructionInfo(value, thisStep.RoundStep);
                updateDisplay();
            }
        }

        public RoundStep Step
        {
            get => thisStep.RoundStep;
            set
            {
                thisStep = new InstructionInfo(thisStep.Team, value);
                updateDisplay();
            }
        }

        public const float WIDTH = 500;
        public const float HEIGHT = 100;

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        private InstructionInfo thisStep;

        private readonly Container iconHolder;
        private Texture? welcomeTexture;

        private readonly TruncatingSpriteText stepName;
        private readonly TruncatingSpriteText stepDescription;

        public InstructionDisplay(TeamColour team = TeamColour.Neutral, RoundStep roundStep = RoundStep.Default)
        {
            thisStep = new InstructionInfo
            (
                team: team,
                roundStep: roundStep
            );

            Height = HEIGHT;
            Width = WIDTH;

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Name = @"Background box",
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f,
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = 20, Vertical = 10 },
                        RowDimensions =
                        [
                            new Dimension(),
                        ],
                        ColumnDimensions =
                        [
                            new Dimension(GridSizeMode.Absolute, 60),
                            new Dimension(GridSizeMode.Absolute, 30),
                            new Dimension(),
                        ],
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                iconHolder = new Container
                                {
                                    Name = @"Icon display",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Box
                                {
                                    Name = @"Separator",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.White,
                                    RelativeSizeAxes = Axes.Y,
                                    Height = 0.75f,
                                    Width = 3,
                                },
                                new FillFlowContainer
                                {
                                    Name = @"Information flow",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        stepName = new TruncatingSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Text = thisStep.Name,
                                            Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                                        },
                                        stepDescription = new TruncatingSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Text = thisStep.Description,
                                            Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Regular),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            welcomeTexture = textures.Get("Icons/welcome-img");
            updateDisplay();
        }

        private void updateDisplay()
        {
            if (!IsLoaded)
                return;

            iconHolder.FadeOut();
            iconHolder.FadeIn(900, Easing.OutQuint);
            iconHolder.ScaleTo(2f).Then().ScaleTo(1, 600, Easing.OutQuint);

            if (thisStep.RoundStep == RoundStep.Default && welcomeTexture != null)
            {
                iconHolder.Child = new Sprite
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = welcomeTexture,
                };
            }
            else
            {
                iconHolder.Child = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = thisStep.Icon,
                    Size = new Vector2(56),
                    Colour = thisStep.IconColour,
                };
            }

            stepName.Text = thisStep.Name;
            stepDescription.Text = thisStep.Description;
        }
    }
}
