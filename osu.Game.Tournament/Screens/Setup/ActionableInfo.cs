// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Setup
{
    internal partial class ActionableInfo : LabelledDrawable<Drawable>
    {
        public const float BUTTON_SIZE = 120;

        public Action? Action;

        protected FillFlowContainer FlowContainer = null!;

        protected OsuButton Button = null!;

        private TournamentSpriteText valueText = null!;

        private SpriteIcon warningIcon = null!;

        public ActionableInfo()
            : base(true)
        {
        }

        public LocalisableString ButtonText
        {
            set => Button.Text = value;
        }

        public LocalisableString Value
        {
            set => valueText.Text = value;
        }

        public bool Failing
        {
            set
            {
                valueText.Colour = value ? Color4.Orange : Color4.White;
                warningIcon.Alpha = value ? 1f : 0f;
            }
        }

        protected override Drawable CreateComponent() => new Container
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        valueText = new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        warningIcon = new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Icon = FontAwesome.Solid.ExclamationTriangle,
                            Colour = Color4.Orange,
                            Alpha = 0,
                            Size = new Vector2(24),
                            Margin = new MarginPadding { Left = 15 },
                        },
                    },
                },
                FlowContainer = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        Button = new RoundedButton
                        {
                            Size = new Vector2(BUTTON_SIZE, 40),
                            Action = () => Action?.Invoke()
                        }
                    }
                }
            }
        };
    }
}
