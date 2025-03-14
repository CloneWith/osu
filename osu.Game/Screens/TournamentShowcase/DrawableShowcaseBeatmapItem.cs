// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Models;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.SelectV2.Leaderboards;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class DrawableShowcaseBeatmapItem : OsuRearrangeableListItem<ShowcaseBeatmap>, IHasContextMenu
    {
        public const float HEIGHT = 120;

        private const float icon_height = 34;

        /// <summary>
        /// Invoked when this item requests to be deleted.
        /// </summary>
        public Action<ShowcaseBeatmap>? RequestDeletion;

        /// <summary>
        /// Invoked when this item requests to be edited.
        /// </summary>
        public Action<ShowcaseBeatmap>? RequestEdit;

        public ShowcaseBeatmap Item
        {
            get => item;
            set
            {
                item = value;
                beatmapInfo = value.BeatmapInfo;
                ruleset = rulesetStore.GetRuleset(value.RulesetId) ?? config.FallbackRuleset.Value;
                var rulesetInstance = ruleset?.CreateInstance();

                if (rulesetInstance != null)
                    requiredMods = value.RequiredMods.ToArray();

                Refresh(needPopulation: true);
            }
        }

        private readonly DelayedLoadWrapper onScreenLoader = new DelayedLoadWrapper(Empty) { RelativeSizeAxes = Axes.Both };

        private readonly ShowcaseConfig config;
        private ShowcaseBeatmap item;
        private WorkingBeatmap? workingBeatmap;
        private IBeatmapInfo? beatmapInfo;
        private IRulesetInfo? ruleset;
        private Mod[] requiredMods = Array.Empty<Mod>();

        private FillFlowContainer difficultyIconContainer = null!;
        private LinkFlowContainer beatmapText = null!;
        private TextFlowContainer difficultyText = null!;
        private LinkFlowContainer authorText = null!;
        private ExplicitContentBeatmapBadge explicitContent = null!;
        private ModDisplay modDisplay = null!;
        private FillFlowContainer buttonsFlow = null!;
        private UpdateableAvatar ownerAvatar = null!;
        private Drawable? editButton;
        private Drawable? removeButton;
        private PanelBackground panelBackground = null!;
        private FillFlowContainer infoFillFlow = null!;
        private Sprite modIcon = null!;
        private Container recordScoreContainer = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private TextureStore textureStore { get; set; } = null!;

        [Resolved]
        private BeatmapSetOverlay? beatmapOverlay { get; set; }

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        public DrawableShowcaseBeatmapItem(ShowcaseBeatmap item, ShowcaseConfig config)
            : base(item)
        {
            this.item = item;
            this.config = config;
            ShowDragHandle.Value = false;
        }

        private async Task populateInfo()
        {
            try
            {
                if (showItemOwner)
                {
                    var foundUser = await userLookupCache.GetUserAsync(item.Selector.Value.OnlineID).ConfigureAwait(false);
                    Schedule(() => ownerAvatar.User = foundUser);
                }

                workingBeatmap = beatmapManager.GetWorkingBeatmap(new BeatmapInfo { ID = item.BeatmapGuid }, true);

                if (ReferenceEquals(workingBeatmap, beatmapManager.DefaultBeatmap))
                {
                    beatmapInfo = await beatmapLookupCache.GetBeatmapAsync(item.BeatmapId).ConfigureAwait(false);
                }
                else
                {
                    beatmapInfo = workingBeatmap.BeatmapInfo;
                }

                ruleset = rulesetStore.GetRuleset(item.RulesetId) ?? config.FallbackRuleset.Value;
                requiredMods = item.RequiredMods.ToArray();

                Scheduler.AddOnce(_ => Refresh(), false);
            }
            catch (Exception e)
            {
                Logger.Log($"Error while populating showcase item {e}");
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onScreenLoader.DelayedLoadStarted += _ =>
            {
                Task.Run(populateInfo);
            };
        }

        private bool allowDeletion = true;

        /// <summary>
        /// Whether this item can be deleted.
        /// </summary>
        public bool AllowDeletion
        {
            get => allowDeletion;
            set
            {
                allowDeletion = value;

                if (removeButton != null)
                    removeButton.Alpha = value ? 1 : 0;
            }
        }

        private bool allowEditing = true;

        /// <summary>
        /// Whether this item can be edited.
        /// </summary>
        public bool AllowEditing
        {
            get => allowEditing;
            set
            {
                allowEditing = value;

                if (editButton != null)
                    editButton.Alpha = value ? 1 : 0;
            }
        }

        private bool showItemOwner = true;

        /// <summary>
        /// Whether to display the avatar of the user which selects the map.
        /// </summary>
        public bool ShowItemOwner
        {
            get => showItemOwner;
            set
            {
                showItemOwner = value;

                ownerAvatar.Alpha = value ? 1 : 0;
            }
        }

        public void Refresh(bool refreshScoreOnly = false, bool needPopulation = false)
        {
            if (needPopulation)
            {
                Task.Run(populateInfo);
            }

            if (!refreshScoreOnly)
            {
                if (beatmapInfo != null)
                {
                    difficultyIconContainer.Child = new DifficultyIcon(beatmapInfo, ruleset, requiredMods)
                    {
                        Size = new Vector2(32),
                        TooltipType = DifficultyIconTooltipType.Extended,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    };
                }
                else
                    difficultyIconContainer.Clear();

                beatmapText.Clear();
                difficultyText.Clear();

                if (beatmapInfo != null)
                {
                    panelBackground.Beatmap.Value = beatmapInfo;
                    beatmapText.AddLink(beatmapInfo.GetDisplayTitleRomanisable(includeDifficultyName: false, includeCreator: false),
                        LinkAction.OpenBeatmap,
                        beatmapInfo.OnlineID.ToString(),
                        null,
                        text =>
                        {
                            text.Truncate = true;
                        });
                    difficultyText.AddText(beatmapInfo.DifficultyName,
                        text =>
                        {
                            text.Truncate = true;
                        });
                }

                authorText.Clear();

                if (!string.IsNullOrEmpty(beatmapInfo?.Metadata.Author.Username))
                {
                    authorText.AddText("mapped by ");
                    authorText.AddUserLink(beatmapInfo.Metadata.Author);
                }

                bool hasExplicitContent = (beatmapInfo?.BeatmapSet as IBeatmapSetOnlineInfo)?.HasExplicitContent == true;
                explicitContent.Alpha = hasExplicitContent ? 1 : 0;

                modDisplay.Current.Value = requiredMods.ToArray();
            }

            recordScoreContainer.Clear();

            recordScoreContainer.Child = item.ShowcaseScore != null
                ? new LeaderboardScoreV2(item.ShowcaseScore, false)
                {
                    ActionOnClick = () => performer?.PerformFromScreen(s => s.Push(new SoloResultsScreen(item.ShowcaseScore)), [typeof(ShowcaseConfigScreen)])
                }
                : new MessagePlaceholder(TournamentShowcaseStrings.NoScoreAssociationPrompt);

            modIcon.Texture = textureStore.Get($"{config.TournamentName}/{item.ModString}{item.ModIndex}");

            buttonsFlow.Clear();
            buttonsFlow.ChildrenEnumerable = createButtons();

            modIcon.FadeInFromZero(500, Easing.OutQuint);
            difficultyIconContainer.FadeInFromZero(500, Easing.OutQuint);
            infoFillFlow.FadeInFromZero(500, Easing.OutQuint);
            recordScoreContainer.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override Drawable CreateContent()
        {
            Action<SpriteText> fontParameters = s => s.Font = OsuFont.Default.With(size: 14, weight: FontWeight.SemiBold);

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = HEIGHT,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            onScreenLoader,
                            panelBackground = new PanelBackground
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Relative, 0.1f),
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        difficultyIconContainer = new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.X,
                                            RelativeSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(4),
                                            Margin = new MarginPadding { Horizontal = 8 },
                                        },
                                        infoFillFlow = new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, -2),
                                            Children = new Drawable[]
                                            {
                                                beatmapText = new LinkFlowContainer(fontParameters)
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    // workaround to ensure only the first line of text shows, emulating truncation (but without ellipsis at the end).
                                                    // TODO: remove when text/link flow can support truncation with ellipsis natively.
                                                    Height = OsuFont.DEFAULT_FONT_SIZE,
                                                    Masking = true,
                                                },
                                                difficultyText = new TextFlowContainer(fontParameters)
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Height = OsuFont.DEFAULT_FONT_SIZE,
                                                    Masking = true,
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10f, 0),
                                                    Children = new Drawable[]
                                                    {
                                                        new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(10f, 0),
                                                            Children = new Drawable[]
                                                            {
                                                                authorText = new LinkFlowContainer(fontParameters) { AutoSizeAxes = Axes.Both },
                                                                explicitContent = new ExplicitContentBeatmapBadge
                                                                {
                                                                    Alpha = 0f,
                                                                    Anchor = Anchor.CentreLeft,
                                                                    Origin = Anchor.CentreLeft,
                                                                    Margin = new MarginPadding { Top = 3f },
                                                                }
                                                            },
                                                        },
                                                        new Container
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            AutoSizeAxes = Axes.Both,
                                                            Child = modDisplay = new ModDisplay
                                                            {
                                                                Scale = new Vector2(0.4f),
                                                                ExpansionMode = ExpansionMode.AlwaysExpanded,
                                                                Margin = new MarginPadding { Vertical = -6 },
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        modIcon = new Sprite
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fit,
                                            Margin = new MarginPadding { Horizontal = 4 },
                                        },
                                        buttonsFlow = new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Direction = FillDirection.Horizontal,
                                            Margin = new MarginPadding { Horizontal = 8 },
                                            AutoSizeAxes = Axes.Both,
                                            Spacing = new Vector2(5),
                                            ChildrenEnumerable = createButtons().Select(button => button.With(b =>
                                            {
                                                b.Anchor = Anchor.Centre;
                                                b.Origin = Anchor.Centre;
                                            })),
                                        },
                                        ownerAvatar = new OwnerAvatar
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Size = new Vector2(icon_height),
                                            Margin = new MarginPadding { Right = 8 },
                                            Masking = true,
                                            CornerRadius = 4,
                                            Alpha = ShowItemOwner ? 1 : 0,
                                        },
                                    }
                                }
                            },
                            recordScoreContainer = new Container
                            {
                                RelativePositionAxes = Axes.Both,
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.5f,
                                Y = 0.5f,
                                Child = item.ShowcaseScore != null
                                    ? new LeaderboardScoreV2(item.ShowcaseScore, false)
                                    : new MessagePlaceholder(TournamentShowcaseStrings.NoScoreAssociationPrompt),
                            }
                        },
                    }
                }
            };
        }

        private IEnumerable<Drawable> createButtons() => new[]
        {
            beatmapInfo == null ? Empty() : new PlaylistDownloadButton(beatmapInfo),
            editButton = new PlaylistEditButton
            {
                Size = new Vector2(30, 30),
                Alpha = AllowEditing ? 1 : 0,
                Action = () => RequestEdit?.Invoke(item),
                TooltipText = CommonStrings.ButtonsEdit,
            },
            removeButton = new PlaylistRemoveButton
            {
                Size = new Vector2(30, 30),
                Alpha = AllowDeletion ? 1 : 0,
                Action = () => RequestDeletion?.Invoke(item),
                TooltipText = TournamentShowcaseStrings.RemoveBeatmap,
            },
        };

        public void UpdateModIcon()
        {
            modIcon.Texture = textureStore.Get($"{config.TournamentName}/{item.ModString}{item.ModIndex}");
            modIcon.FadeInFromZero(500, Easing.OutQuint);
        }

        public async Task UpdateOwnerAvatar()
        {
            if (showItemOwner)
            {
                var foundUser = await userLookupCache.GetUserAsync(item.Selector.Value.OnlineID).ConfigureAwait(false);
                Schedule(() => ownerAvatar.User = foundUser);
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            panelBackground.FadeColour(OsuColour.Gray(0.7f), BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            panelBackground.FadeColour(OsuColour.Gray(1f), BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (beatmapOverlay != null)
                    items.Add(new OsuMenuItem(TournamentShowcaseStrings.ShowBeatmapDetails, MenuItemType.Standard, () => beatmapOverlay.FetchAndShowBeatmap(item.BeatmapId)));

                if (item.ShowcaseScore != null)
                {
                    items.Add(new OsuMenuItem(TournamentShowcaseStrings.RemoveReplayScore, MenuItemType.Highlighted, () =>
                    {
                        item.ShowcaseScore = null;
                        recordScoreContainer.Child = new MessagePlaceholder(TournamentShowcaseStrings.NoScoreAssociationPrompt);
                    }));
                }

                return items.ToArray();
            }
        }

        public partial class PlaylistEditButton : GrayButton
        {
            public PlaylistEditButton()
                : base(FontAwesome.Solid.Edit)
            {
            }
        }

        public partial class PlaylistRemoveButton : GrayButton
        {
            public PlaylistRemoveButton()
                : base(FontAwesome.Solid.MinusSquare)
            {
            }
        }

        private sealed partial class PlaylistDownloadButton : BeatmapDownloadButton
        {
            private readonly IBeatmapInfo beatmap;

            [Resolved]
            private BeatmapManager beatmapManager { get; set; } = null!;

            // required for download tracking, as this button hides itself. can probably be removed with a bit of consideration.
            public override bool IsPresent => true;

            private const float width = 50;

            public PlaylistDownloadButton(IBeatmapInfo beatmap)
                : base(beatmap.BeatmapSet)
            {
                this.beatmap = beatmap;

                Size = new Vector2(width, 30);
                Alpha = 0;
            }

            protected override void LoadComplete()
            {
                State.BindValueChanged(stateChanged, true);

                // base implementation calls FinishTransforms, so should be run after the above state update.
                base.LoadComplete();
            }

            private void stateChanged(ValueChangedEvent<DownloadState> state)
            {
                switch (state.NewValue)
                {
                    case DownloadState.Unknown:
                        // Ignore initial state to ensure the button doesn't briefly appear.
                        break;

                    case DownloadState.LocallyAvailable:
                        // Perform a local query of the beatmap by beatmap checksum, and reset the state if not matching.
                        if (beatmapManager.QueryBeatmap(b => b.MD5Hash == beatmap.MD5Hash) == null)
                            State.Value = DownloadState.NotDownloaded;
                        else
                        {
                            this.FadeTo(0, 500)
                                .ResizeWidthTo(0, 500, Easing.OutQuint);
                        }

                        break;

                    default:
                        this.ResizeWidthTo(width, 500, Easing.OutQuint)
                            .FadeTo(1, 500);
                        break;
                }
            }
        }

        // For now, this is the same implementation as in PanelBackground, but supports a beatmap info rather than a working beatmap
        private partial class PanelBackground : Container // todo: should be a buffered container (https://github.com/ppy/osu-framework/issues/3222)
        {
            public readonly Bindable<IBeatmapInfo> Beatmap = new Bindable<IBeatmapInfo>();

            public PanelBackground()
            {
                UpdateableBeatmapBackgroundSprite backgroundSprite;

                InternalChildren = new Drawable[]
                {
                    backgroundSprite = new UpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Depth = -1,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                        Shear = new Vector2(0.8f, 0),
                        Alpha = 0.6f,
                        Children = new[]
                        {
                            // The left half with no gradient applied
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Width = 0.4f,
                            },
                            // Piecewise-linear gradient with 2 segments to make it appear smoother
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.7f)),
                                Width = 0.4f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.7f), new Color4(0, 0, 0, 0.4f)),
                                Width = 0.4f,
                            },
                        }
                    }
                };

                // manual binding required as playlists don't expose IBeatmapInfo currently.
                // may be removed in the future if this changes.
                Beatmap.BindValueChanged(beatmap => backgroundSprite.Beatmap.Value = beatmap.NewValue);
            }
        }

        private partial class OwnerAvatar : UpdateableAvatar, IHasTooltip
        {
            public OwnerAvatar()
            {
                AddInternal(new TooltipArea(this)
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1
                });
            }

            public LocalisableString TooltipText => User == null ? string.Empty : $"selected by {User.Username}";

            private partial class TooltipArea : Component, IHasTooltip
            {
                private readonly OwnerAvatar avatar;

                public TooltipArea(OwnerAvatar avatar)
                {
                    this.avatar = avatar;
                }

                public LocalisableString TooltipText => avatar.TooltipText;
            }
        }
    }
}
