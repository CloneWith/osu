// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneBoxResizing : TournamentTestScene
    {
        private readonly Container topContainer;
        private readonly TournamentSpriteText topText;

        public TestSceneBoxResizing()
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 200,
                Height = 200,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new TournamentSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = @"Base content",
                        Colour = Color4.Black,
                        Shadow = false,
                    },
                    topContainer = new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        Height = 0,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.SkyBlue,
                            },
                            topText = new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = @"Floating content",
                                Alpha = 0,
                                Colour = Color4.Black,
                                Shadow = false,
                            }
                        }
                    }
                },
            });
        }

        [Test]
        public void TestTopResizingBox()
        {
            AddStep("Resize top container to 100%", () => topContainer.ResizeHeightTo(1, 1000, Easing.InOutQuint));
            AddStep("Show text", () => topText.FadeIn(500, Easing.OutQuint));

            AddStep("Change anchor to top centre", () =>
            {
                topContainer.Anchor = Anchor.TopCentre;
                topContainer.Origin = Anchor.TopCentre;
            });

            AddStep("Hide text", () => topText.FadeOut(500, Easing.OutQuint));
            AddStep("Resize top container to 0%", () => topContainer.ResizeHeightTo(0, 1000, Easing.InOutQuint));

            AddStep("Change anchor to bottom centre", () =>
            {
                topContainer.Anchor = Anchor.BottomCentre;
                topContainer.Origin = Anchor.BottomCentre;
            });
        }
    }
}
