// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.FumoUserInterface
{
    public partial class TestSceneFumoColours : OsuTestScene
    {
        [Test]
        public void TestFumoColours()
        {
            AddStep("Load colour displays", () =>
            {
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10f),
                    Children = Enum.GetValues<FumoColours.Theme>()
                                   .Select(theme => new ColourLine(theme))
                                   .ToArray(),
                };
            });
        }

        public partial class ColourLine : FillFlowContainer
        {
            public ColourLine(FumoColours.Theme theme)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(10f);

                var colour = FumoColours.FromTheme(theme);

                ChildrenEnumerable = new[]
                    {
                        colour.Darkest,
                        colour.Darker,
                        colour.Dark,
                        colour.Regular,
                        colour.Light,
                        colour.Lighter,
                        colour.Lightest,
                    }.Select<Color4, Drawable>(c => new ColourItem(c))
                     .Append(new OsuSpriteText
                     {
                         Anchor = Anchor.CentreLeft,
                         Origin = Anchor.CentreLeft,
                         Text = theme.GetLocalisableDescription(),
                     });
            }
        }

        public partial class ColourItem : FillFlowContainer
        {
            public ColourItem(Color4 colour, string label = "")
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(10);

                Children = new Drawable[]
                {
                    new CircularContainer
                    {
                        Masking = true,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(75f, 25f),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colour,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = OsuColour.ForegroundTextColourFor(colour),
                                Text = colour.ToHex(),
                            },
                        }
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = label,
                    }
                };
            }
        }
    }
}
