// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TeamCoupletCounter : StarCounter
    {
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
        private readonly InnerCounter counter;

        public TeamCoupletCounter(Bindable<int?>? score, TeamColour colour, int count)
        {
            bool flip = colour == TeamColour.Blue;

            AutoSizeAxes = Axes.Both;

            InternalChild = counter = new InnerCounter(colour, count)
            {
                AutoSizeAxes = Axes.Both,
                Anchor = flip ? Anchor.TopRight : Anchor.TopLeft,
                Scale = flip ? new Vector2(-1, 1) : Vector2.One,
            };

            currentTeamScore.BindValueChanged(scoreChanged);
            currentTeamScore.BindTo(score);
        }

        private void scoreChanged(ValueChangedEvent<int?> score) => counter.Current = score.NewValue ?? 0;

        public partial class InnerCounter : StarCounter
        {
            private readonly Color4 accentColour;

            public override Star CreateStar() => new CoupletCircle(accentColour);

            public InnerCounter(TeamColour colour, int count)
                : base(count)
            {
                accentColour = colour switch
                {
                    TeamColour.Red => FumoColours.FlandreRed.Regular,
                    TeamColour.Blue => FumoColours.SeaBlue.Regular,
                    _ => FumoColours.SunshineYellow.Regular,
                };
            }

            public partial class CoupletCircle : Star
            {
                private readonly CircularContainer content;
                private readonly CircularContainer circle;
                private const float idle_size = 15f;
                private const float active_size = 20f;

                public CoupletCircle(Color4 colour)
                {
                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;
                    Size = new Vector2(active_size);

                    InternalChildren = new Drawable[]
                    {
                        content = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(active_size),
                            Masking = true,
                            BorderColour = colour,
                            BorderThickness = 2,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Transparent,
                                    RelativeSizeAxes = Axes.Both,
                                    AlwaysPresent = true,
                                },
                                circle = new Circle
                                {
                                    Colour = colour,
                                    RelativeSizeAxes = Axes.Both,

                                },
                            },
                        },
                    };

                    Masking = true;
                }

                public override void DisplayAt(float scale)
                {
                    if (scale == 1)
                    {
                        circle.FlashColour(Color4.White, 1000, Easing.OutQuint);
                    }

                    float targetSize = idle_size + (active_size - idle_size) * Math.Clamp(scale, 0, 1);
                    content.ResizeTo(new Vector2(targetSize), 300, Easing.OutQuint);
                    circle.FadeTo(scale, 500, Easing.OutQuint);
                }
            }
        }
    }
}
