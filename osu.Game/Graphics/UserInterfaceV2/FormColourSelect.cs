// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormColourSelect : CompositeDrawable, IHasCurrentValue<Colour4>, IHasPopover, IFormControl
    {
        public LocalisableString PaletteHeaderText { get; init; } = string.Empty;

        public Bindable<Colour4> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        /// <summary>
        /// Optional colours offered as presets in each colour select popover.
        /// </summary>
        public BindableList<Colour4> Suggestions { get; } = new BindableList<Colour4>();

        public LocalisableString Caption { get; init; }
        public LocalisableString HintText { get; init; }

        private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>();
        private readonly Bindable<Visibility> popoverState = new Bindable<Visibility>();

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
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            popoverState.BindValueChanged(_ => updateState());
            current.BindDisabledChanged(_ => updateState());
            current.BindValueChanged(e =>
            {
                label.Text = e.NewValue.ToHex();
                colourPreview.Colour = e.NewValue;

                ValueChanged?.Invoke();
            }, true);

            updateState();
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
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

        private void updateState()
        {
            caption.Colour = colourProvider.Content2;
            label.Colour = Current.Disabled ? colourProvider.Foreground1 : colourProvider.Content1;

            if (Current.Disabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (popoverState.Value == Visibility.Visible)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        public Popover GetPopover()
        {
            var popover = new ColourPickerPopover(Suggestions)
            {
                PaletteHeaderText = PaletteHeaderText,
                Current = { BindTarget = Current }
            };

            popoverState.UnbindBindings();
            popoverState.BindTo(popover.State);
            return popover;
        }

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;

        private partial class ColourPickerPopover : OsuPopover, IHasCurrentValue<Colour4>
        {
            public LocalisableString PaletteHeaderText { get; init; } = string.Empty;

            public Bindable<Colour4> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>();
            private readonly BindableList<Colour4> suggestions;

            public ColourPickerPopover(BindableList<Colour4> suggestions)
                : base(false)
            {
                this.suggestions = suggestions;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                var picker = new OsuColourPicker
                {
                    PaletteHeaderText = PaletteHeaderText,
                    Current = { BindTarget = Current },
                };
                picker.Suggestions.BindTo(suggestions);
                Child = picker;

                Body.BorderThickness = 2;
                Body.BorderColour = colourProvider.Highlight1;
                Content.Padding = new MarginPadding(2);
            }
        }
    }
}
