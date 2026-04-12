// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    /// <summary>
    /// An expandable section header that supports coloured icons and dynamic content changing.
    /// </summary>
    public partial class FumoSectionHeader : CompositeDrawable
    {
        /// <summary>
        /// The icon to show at the start of the header.
        /// </summary>
        /// <remarks>Setting to new <see cref="IconUsage()"/> can hide the icon.</remarks>
        public IconUsage Icon
        {
            get => icon.Value;
            set => icon.Value = value;
        }

        /// <summary>
        /// The text of the header.
        /// </summary>
        public LocalisableString Text
        {
            get => text.Value;
            set => text.Value = value;
        }

        /// <summary>
        /// The theme colour of the header.
        /// </summary>
        /// <remarks>This would be used by the icon and the bottom line (with 50% alpha).</remarks>
        public Color4 AccentColour
        {
            get => accentColour.Value;
            set => accentColour.Value = value;
        }

        /// <summary>
        /// The size of the icon.
        /// </summary>
        public int IconSize
        {
            get => iconSize.Value;
            set => iconSize.Value = value;
        }

        /// <summary>
        /// The font style the header text should use.
        /// </summary>
        public FontUsage Font
        {
            get => textFont.Value;
            set => textFont.Value = value;
        }

        private readonly Bindable<IconUsage> icon = new Bindable<IconUsage>();
        private readonly BindableInt iconSize = new BindableInt(24);
        private readonly Bindable<LocalisableString> text = new Bindable<LocalisableString>();
        private readonly Bindable<Color4> accentColour = new Bindable<Color4>(FumoColours.SeaBlue.Regular);
        private readonly Bindable<FontUsage> textFont = new Bindable<FontUsage>(OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold));

        private Circle bottomLine = null!;
        private Container iconContainer = null!;
        private SpriteIcon headerIcon = null!;
        private TruncatingSpriteText headerText = null!;

        public FumoSectionHeader()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                bottomLine = new Circle
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 4,
                    Colour = accentColour.Value,
                    Alpha = 0.5f,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizeEasing = Easing.OutQuint,
                    AutoSizeDuration = 300,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Padding = new MarginPadding { Horizontal = 5 },
                    Children = new Drawable[]
                    {
                        iconContainer = new Container
                        {
                            Name = @"Icon container",
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Size = new Vector2(iconSize.Value),
                            Alpha = Icon.Equals(default) ? 0 : 1,
                            Child = headerIcon = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = accentColour.Value,
                                Icon = icon.Value,
                            },
                        },
                        headerText = new TruncatingSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Text = text.Value,
                            Font = textFont.Value,
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            icon.BindValueChanged(v =>
            {
                if (v.NewValue.Equals(default))
                {
                    iconContainer.FadeOut(500, Easing.OutQuint);
                }
                else
                {
                    iconContainer.FadeIn(500, Easing.OutQuint);
                    headerIcon.Icon = v.NewValue;
                    headerIcon.ScaleTo(0.8f).Then().ScaleTo(1, 500, Easing.OutQuint);
                }
            });
            iconSize.BindValueChanged(s =>
                iconContainer.ResizeTo(new Vector2(s.NewValue), 500, Easing.OutQuint));
            text.BindValueChanged(t => headerText.Text = t.NewValue);
            textFont.BindValueChanged(f => headerText.Font = f.NewValue);
            accentColour.BindValueChanged(c =>
            {
                headerIcon.FadeColour(c.NewValue, 500, Easing.OutQuint);
                bottomLine.FadeColour(c.NewValue, 500, Easing.OutQuint);
            });
        }
    }
}
