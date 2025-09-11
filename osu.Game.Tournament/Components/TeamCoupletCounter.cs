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
    public partial class TeamCoupletCounter : Container
    {
        /// <summary>
        /// The current score bindable.
        /// </summary>
        public Bindable<int?> Current = new Bindable<int?>();

        /// <summary>
        /// How many circles should we display.
        /// </summary>
        public int CircleCount
        {
            get => count;
            set
            {
                if (value < 0)
                    return;

                count = value;
                setInnerCounter();
            }
        }

        private InnerCounter counter = null!;
        private readonly TeamColour colour;

        private int count;

        public TeamCoupletCounter(TeamColour colour)
        {
            this.colour = colour;

            AutoSizeAxes = Axes.Both;
            setInnerCounter();

            Current.BindValueChanged(scoreChanged);
        }

        private void scoreChanged(ValueChangedEvent<int?> score) => counter.Current = score.NewValue ?? 0;

        private void setInnerCounter()
        {
            bool flip = colour == TeamColour.Blue;
            InternalChild = counter = new InnerCounter(colour, count)
            {
                AutoSizeAxes = Axes.Both,
                Anchor = flip ? Anchor.TopRight : Anchor.TopLeft,
                Scale = flip ? new Vector2(-1, 1) : Vector2.One,
            };
        }

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
                    float targetSize = idle_size + (active_size - idle_size) * Math.Clamp(scale, 0, 1);
                    content.ResizeTo(new Vector2(targetSize), 300, Easing.OutQuint);
                    circle.FadeTo(scale, 500, Easing.OutQuint);
                }
            }
        }
    }
}
