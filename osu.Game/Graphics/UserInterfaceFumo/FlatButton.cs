// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    /// <summary>
    /// A button with added default sound effects.
    /// </summary>
    public abstract partial class FlatButton : Button
    {
        public LocalisableString Text
        {
            get => SpriteText.Text;
            set => SpriteText.Text = value;
        }

        private Color4? backgroundColour;

        /// <summary>
        /// Sets a custom background colour to this button, replacing the provided default.
        /// </summary>
        public virtual Color4 BackgroundColour
        {
            get => backgroundColour ?? DefaultBackgroundColour;
            set => backgroundColour = value;
        }

        /// <summary>
        /// Sets a default background colour to this button.
        /// </summary>
        protected Color4 DefaultBackgroundColour { get; set; }

        protected override Container<Drawable> Content { get; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            // base call is checked for cases when `OsuClickableContainer` has masking applied to it directly (ie. externally in object initialisation).
            base.ReceivePositionalInputAt(screenSpacePos)
            // Implementations often apply masking / edge rounding at a content level, so it's imperative to check that as well.
            && Content.ReceivePositionalInputAt(screenSpacePos);

        protected Box Background;
        protected SpriteText SpriteText;

        private readonly Box flashLayer;
        private readonly Box topMask;

        protected FlatButton(HoverSampleSet? hoverSounds = HoverSampleSet.Button)
        {
            Height = 40;

            AddInternal(Content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Background = new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                        Colour = Color4.White.Opacity(0)
                    },
                    SpriteText = CreateText(),
                    topMask = new Box
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                        Blending = BlendingParameters.Mixture,
                        Depth = float.MinValue
                    },
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive,
                        Depth = float.MinValue,
                        Colour = Color4.White.Opacity(0.5f),
                        Alpha = 0,
                    }
                }
            });

            if (hoverSounds.HasValue)
                AddInternal(new HoverClickSounds(hoverSounds.Value) { Enabled = { BindTarget = Enabled } });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DefaultBackgroundColour = colours.BlueDark;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(e => topMask.FadeTo(e.NewValue ? 0 : 0.5f, 200, Easing.OutQuint));
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value)
                flashLayer.FadeOutFromOne(300, Easing.OutQuint);

            return base.OnClick(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Background.FadeColour(BackgroundColour, 800, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Background.FadeColour(BackgroundColour.Opacity(IsHovered ? 0.3f : 0), 800, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected virtual SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = OsuFont.GetFont(weight: FontWeight.Bold)
        };
    }
}
