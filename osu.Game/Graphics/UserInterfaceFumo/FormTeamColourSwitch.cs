// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceFumo
{
    public partial class FormTeamColourSwitch : CompositeDrawable, IHasCurrentValue<TeamColour>, IFormControl
    {
        public Bindable<TeamColour> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<TeamColour> current = new BindableWithCurrent<TeamColour>();

        public LocalisableString Caption { get; init; }
        public LocalisableString HintText { get; init; }

        private FormControlBackground background = null!;
        private FormFieldCaption caption = null!;
        private TruncatingSpriteText label = null!;
        private Circle colourPreview = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(4),
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                caption = new FormFieldCaption
                                {
                                    Caption = Caption,
                                    TooltipText = HintText,
                                },
                                label = new TruncatingSpriteText
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Padding = new MarginPadding { Right = 25 },
                                    AlwaysPresent = true,
                                },
                            },
                        },
                        colourPreview = new Circle
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Size = new Vector2(16),
                            Margin = new MarginPadding { Right = 5 },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindDisabledChanged(_ => updateState());
            current.BindValueChanged(e =>
            {
                updateState();
                background.FlashOnCommit();

                ValueChanged?.Invoke();
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override bool OnClick(ClickEvent e)
        {
            current.Value = current.Value switch
            {
                TeamColour.Red => TeamColour.Blue,
                _ => TeamColour.Red,
            };

            return true;
        }

        private void updateState()
        {
            caption.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content2;
            label.Colour = Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content1;
            label.Text = TournamentExtensions.GetTeamString(current.Value);
            colourPreview.FadeColour(TournamentExtensions.GetTeamColour(current.Value), 300, Easing.OutQuint);

            if (IsDisabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;
    }
}
