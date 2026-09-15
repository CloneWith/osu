// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    internal partial class WarningBox : Container
    {
        /// <summary>
        /// Whether the warning box could be closed on click.
        /// </summary>
        public bool Dismissable;

        public IconUsage Icon
        {
            get => warningIcon.Icon;
            set => warningIcon.Icon = value;
        }

        public LocalisableString Text
        {
            get => warningText.Text;
            set => warningText.Text = value;
        }

        private readonly SpriteIcon warningIcon;
        private readonly TournamentSpriteText warningText;

        public WarningBox()
        {
            Masking = true;
            CornerRadius = 5;
            Depth = float.MinValue;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Orange.Opacity(0.6f),
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    Children = new Drawable[]
                    {
                        warningIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = FontAwesome.Solid.ExclamationTriangle,
                            Colour = Color4.White,
                            Size = new Vector2(24),
                        },
                        warningText = new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                            Colour = Color4.White,
                        },
                    }
                },
            };
        }

        public WarningBox(LocalisableString text)
            : this()
        {
            warningText.Text = text;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Dismissable)
            {
                this.FadeOut(300, Easing.OutQuint);
                return true;
            }

            return base.OnClick(e);
        }
    }
}
