// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A simple dual-colour background layer with subtle animations.
    /// </summary>
    public partial class TeamGradientBackground : CompositeDrawable
    {
        private const float move_duration = 10000;
        private const float fade_duration = 15000;

        private readonly Box redArea, blueArea;

        public TeamGradientBackground()
        {
            Name = "Gradient Background";
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                redArea = new Box
                {
                    Name = "Red area",
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.3f,
                    Shear = OsuGame.SHEAR,
                    Colour = ColourInfo.GradientHorizontal(TournamentGame.COLOUR_RED,
                        TournamentGame.COLOUR_RED.Opacity(0)),
                },
                blueArea = new Box
                {
                    Name = "Blue area",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.3f,
                    Shear = OsuGame.SHEAR,
                    Colour = ColourInfo.GradientHorizontal(TournamentGame.COLOUR_BLUE.Opacity(0),
                        TournamentGame.COLOUR_BLUE),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Horizontal move animation
            redArea.Loop(move_duration, t =>
                t.MoveToX(-100, move_duration, Easing.InOutSine)
                 .Then(move_duration).MoveToX(0, move_duration, Easing.InOutSine)
            );
            blueArea.Loop(move_duration, t =>
                t.MoveToX(100, move_duration, Easing.InOutSine)
                 .Then(move_duration).MoveToX(0, move_duration, Easing.InOutSine)
            );

            // Fading animation
            redArea.Loop(1000, t =>
                t.FadeTo(0.5f, fade_duration, Easing.InOutSine)
                 .Then(3000).FadeTo(1, fade_duration, Easing.InOutSine)
            );
            blueArea.Loop(1000, t =>
                t.FadeTo(0.5f, fade_duration, Easing.InOutSine)
                 .Then(3000).FadeTo(1, fade_duration, Easing.InOutSine)
            );
        }
    }
}
