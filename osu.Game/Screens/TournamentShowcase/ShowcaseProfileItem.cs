using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseProfileItem : CompositeDrawable, IHasContextMenu
    {
        protected const float CORNER_RADIUS = 10;
        private const float height = 80;
        private const float indicator_height_active = 18;
        private const float indicator_height_inactive = 4;

        public Action? OnLaunch;

        public Action? OnEdit;

        public BindableBool Focused { get; } = new BindableBool();

        [Cached]
        private readonly OverlayColourProvider colourProvider;

        private readonly string filename;
        private readonly ShowcaseConfig config;

        private readonly FormControlBackground background;
        private readonly CircularContainer selectionIndicator;
        private readonly Container iconContainer;
        private readonly OsuTextFlowContainer titleFlow;
        private readonly OsuTextFlowContainer detailsFlow;

        public ShowcaseProfileItem(string filename, ShowcaseConfig config)
        {
            this.filename = filename;
            this.config = config;
            colourProvider = new OverlayColourProvider(config.ColourScheme.Value);

            Masking = true;
            CornerRadius = CORNER_RADIUS;
            RelativeSizeAxes = Axes.X;
            Height = height;

            InternalChildren = new Drawable[]
            {
                background = new ControlBackground(),
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            selectionIndicator = new CircularContainer
                            {
                                Width = 4,
                                Height = indicator_height_inactive,
                                Masking = true,
                                CornerRadius = 1.5f,
                                Colour = colourProvider.Highlight1,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 15, Right = 10 },
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.White,
                                },
                            },
                            iconContainer = new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Right = 15 },
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(2),
                                Margin = new MarginPadding { Vertical = 2 },
                                Children = new Drawable[]
                                {
                                    titleFlow = new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Title)
                                    {
                                        Name = @"Profile Name",
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        TextAnchor = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                    },
                                    detailsFlow = new OsuTextFlowContainer(t =>
                                    {
                                        t.Font = OsuFont.Style.Body.With(size: 18);
                                        t.Margin = new MarginPadding { Right = 5 };
                                    })
                                    {
                                        Name = @"Details",
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        TextAnchor = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(IRulesetStore rulesetStore)
        {
            var ruleset = rulesetStore.GetRuleset(config.FallbackRuleset.Value.OnlineID)?.CreateInstance();

            var icon = ruleset?.CreateIcon();

            iconContainer.Child = icon?.With(i => i.Size = new Vector2(24))
                                  ?? new SpriteIcon
                                  {
                                      Size = new Vector2(24),
                                      Icon = FontAwesome.Regular.Circle,
                                  };

            var creationParameters = (SpriteText i) =>
            {
                i.Size = new Vector2(20);
                i.Margin = new MarginPadding { Right = 5 };
            };

            titleFlow.AddText(config.TournamentName.Value, t => t.Colour = colourProvider.Highlight1);
            titleFlow.AddText(@$" [{config.RoundName.Value}] ");
            titleFlow.AddText($@"({filename})", t => t.Font = OsuFont.Style.Caption1);

            detailsFlow.AddIcon(FontAwesome.Solid.Music, creationParameters);
            detailsFlow.AddText($"{config.Beatmaps.Count}");

            if (config.LastEdited.Value != null)
            {
                detailsFlow.AddIcon(FontAwesome.Solid.PencilAlt, creationParameters);
                detailsFlow.AddArbitraryDrawable(new DrawableDate(config.LastEdited.Value.Value, 18f, false));
            }

            if (config.LastPlayed.Value != null)
            {
                detailsFlow.AddIcon(FontAwesome.Solid.Play);
                detailsFlow.AddArbitraryDrawable(new DrawableDate(config.LastPlayed.Value.Value, 18f, false));
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            Focused.Value = !Focused.Value;
            background.FlashOnCommit();
            updateState();
            return true;
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            OnEdit?.Invoke();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override void OnFocus(FocusEvent e)
        {
            updateState();
            Focused.Value = true;
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            updateState();
            Focused.Value = false;
            base.OnFocusLost(e);
        }

        private void updateState()
        {
            selectionIndicator.ResizeHeightTo(Focused.Value || IsHovered ? indicator_height_active : indicator_height_inactive, 500, Easing.OutQuint);

            if (Focused.Value)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        // TODO: Add implementation
        public MenuItem[] ContextMenuItems =>
        [
            new OsuMenuItem(TournamentShowcaseStrings.StartShowcase, MenuItemType.Highlighted, OnLaunch),
            new OsuMenuItem(ButtonSystemStrings.Edit.ToSentence(), MenuItemType.Standard, OnEdit),
            new OsuMenuItemSpacer(),
            new OsuMenuItem(CommonStrings.Clone),
            new OsuMenuItem(CommonStrings.Rename),
            new OsuMenuItem(CommonStrings.DeleteWithConfirmation, MenuItemType.Destructive),
        ];

        private partial class ControlBackground : FormControlBackground
        {
            public ControlBackground()
            {
                CornerRadius = CORNER_RADIUS;
            }
        }
    }
}
