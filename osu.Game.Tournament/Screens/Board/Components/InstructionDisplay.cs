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
        public const float WIDTH = 500;
        public const float HEIGHT = 100;

        private readonly InstructionInfo thisStep;

        private readonly Container iconHolder;

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
                            new Dimension(GridSizeMode.AutoSize),
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
                                    Size = new Vector2(56),
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
                                        new TruncatingSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Text = thisStep.Name,
                                            Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                                        },
                                        new TruncatingSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Text = thisStep.Description,
                                            Font = OsuFont.Torus.With(size: 30, weight: FontWeight.Regular),
                                        }
                                    }
                                }
                            },
                        }
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture? welcomeTexture = textures.Get("Icons/welcome-img");

            if (thisStep.RoundStep == RoundStep.Default && welcomeTexture != null)
            {
                iconHolder.Child = new Sprite
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = HEIGHT - 10 * 2,
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
                    Colour = thisStep.IconColor,
                };
            }
        }
    }
}
