// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Tournament.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    /// <summary>
    /// A virtual board block component to connect the board and chess pieces.
    /// </summary>
    public partial class DrawableBoardBlock : CompositeDrawable
    {
        public const int DEFAULT_WIDTH = 200;

        public readonly int BoardRow;
        public readonly int BoardColumn;

        public Container<FumoChessPiece> ChessLayer { get; private set; } = null!;

        private const int border_duration = 300;
        private const int transform_duration = 1000;

        private EmptyBox backgroundLayer = null!;
        private SpriteIcon iconLayer = null!;

        /// <inheritdoc cref="DrawableBoardBlock"/>
        /// <param name="row">the row of the block representation.</param>
        /// <param name="column">the column of the block representation.</param>
        public DrawableBoardBlock(int row, int column)
        {
            BoardRow = row;
            BoardColumn = column;

            Width = DEFAULT_WIDTH;
            Height = DEFAULT_WIDTH;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                backgroundLayer = new EmptyBox
                {
                    Name = @"Background layer",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    BoxColour = Color4.White.Opacity(0),
                    BorderColour = Color4.White,
                },
                iconLayer = new SpriteIcon
                {
                    Name = @"Icon layer",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(48),
                    Alpha = 0,
                },
                ChessLayer = new Container<FumoChessPiece>
                {
                    Name = @"Chess layer",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Height = 0.8f,
                },
            };
        }

        /// <summary>
        /// Fade the background to a specific colour.
        /// </summary>
        /// <param name="colour">the <see cref="Color4"/> to fade to.</param>
        /// <param name="duration">the length of the animation.</param>
        /// <remarks>the colour of the flash would be dimmed by 25%.</remarks>
        public void FadeBackgroundColour(Color4? colour = null, int duration = transform_duration)
        {
            backgroundLayer.FinishTransforms();
            backgroundLayer.TransformTo(nameof(backgroundLayer.BoxColour), colour?.Opacity(0.75f) ?? Color4.White.Opacity(0), duration, Easing.OutQuint);
        }

        /// <summary>
        /// Flash the background of the block as a visual cue.
        /// </summary>
        /// <param name="colour">the <see cref="Color4"/> of the flash.</param>
        /// <param name="duration">the length of the flash.</param>
        /// <remarks>the colour of the flash would be dimmed by 25%.</remarks>
        public void FlashColour(Color4? colour = null, int duration = transform_duration)
        {
            backgroundLayer.FinishTransforms();
            backgroundLayer.BoxColour = colour?.Opacity(0.75f) ?? Color4.White.Opacity(0.75f);
            backgroundLayer.TransformTo(nameof(backgroundLayer.BoxColour), Color4.White.Opacity(0), duration, Easing.OutQuint);
        }

        /// <summary>
        /// Make an icon appear for a short time on the block.
        /// </summary>
        /// <param name="icon">the <see cref="IconUsage"/> to show.</param>
        /// <param name="colour">the colour of the icon.</param>
        /// <param name="duration">the length of the icon's appearance.</param>
        public void FlashIcon(IconUsage icon, Color4? colour = null, int duration = transform_duration)
        {
            iconLayer.FinishTransforms();
            iconLayer.Icon = icon;
            iconLayer.Colour = colour ?? Color4.White;

            iconLayer.FadeOutFromOne(duration, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundLayer.TransformTo(nameof(backgroundLayer.BorderThickness), 3f, border_duration, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundLayer.TransformTo(nameof(backgroundLayer.BorderThickness), 0f, border_duration, Easing.OutQuint);

            base.OnHoverLost(e);
        }
    }
}
