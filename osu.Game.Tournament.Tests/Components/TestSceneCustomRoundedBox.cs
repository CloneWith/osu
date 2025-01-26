// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneCustomRoundedBox : TournamentTestScene
    {
        private readonly CustomRoundedBoxBase customRoundedBoxBase;
        private readonly CustomRoundedBox roundedBoxWithText;

        public TestSceneCustomRoundedBox()
        {
            Children = new Drawable[]
            {
                customRoundedBoxBase = new CustomRoundedBoxBase
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.75f,
                    Height = 0.1f
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        roundedBoxWithText = new CustomRoundedBox
                        {
                            Child = new OsuSpriteText
                            {
                                Text = "Hello shaders!",
                                Font = OsuFont.TorusAlternate.With(size: 24, weight: FontWeight.SemiBold),
                                Padding = new MarginPadding
                                {
                                    Horizontal = 10, Vertical = 2
                                }
                            }
                        },
                        new CustomRoundedBox
                        {
                            Child = new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(4),
                                Padding = new MarginPadding
                                {
                                    Horizontal = 8
                                },
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Icon = FontAwesome.Regular.Clipboard,
                                        Size = new Vector2(20),
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "Custom rounded box!",
                                        Font = OsuFont.TorusAlternate.With(size: 20, weight: FontWeight.SemiBold),
                                    }
                                }
                            }
                        }
                    },
                }
            };
        }

        [Test]
        public void TestBaseProperties()
        {
            AddStep("Set background colour", () => customRoundedBoxBase.BackgroundColour = Color4.SkyBlue);
            AddAssert("Ensure correct Filling mode", () => customRoundedBoxBase.FillMode == FillMode.Fill);
            AddStep("Invalidate and reload", () => customRoundedBoxBase.Invalidate());
            AddSliderStep("Width", 0, 1, 0.75f, v => customRoundedBoxBase.Width = v);
            AddSliderStep("Height", 0, 1, 0.1f, v => customRoundedBoxBase.Height = v);
        }

        [Test]
        public void TestBoxProperties()
        {
            AddStep("Set background colour", () => roundedBoxWithText.BackgroundColour = FumoColours.FlandreRed.Regular);
        }
    }
}
