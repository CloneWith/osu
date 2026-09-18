// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.RandomPick.Components
{
    /// <summary>
    /// One team's box on the random pick screen: the team, and the player it has drawn from each tier.
    /// </summary>
    public partial class PickerGroup : Container
    {
        public const float WIDTH = 280;

        /// <summary>
        /// The height of the box. It is fixed because the boxes are laid out in two rows which grow towards the
        /// middle of the screen, so they all have to agree on how tall they are.
        /// </summary>
        public const float HEIGHT = 210;

        private const float slot_spacing = 4;
        public const float SLOT_HEIGHT = 48;

        public readonly TournamentTeam Team;

        /// <summary>
        /// Raised when the box is left-clicked, i.e. the operator wants the next draw to come here.
        /// </summary>
        public event Action<PickerGroup>? OnClicked;

        private readonly BindableList<TournamentUser> roster;
        private readonly Bindable<string> fullName;
        private readonly Bindable<string> acronym;

        /// <summary>
        /// The front-row players whose arrival has not been shown yet. A player arriving in a box is only ever
        /// the random phase placing them, and which position just changed is what the operator watches for.
        /// </summary>
        private readonly HashSet<TournamentUser> unannounced = new HashSet<TournamentUser>();

        private FormControlBackground background = null!;
        private TournamentSpriteText titleText = null!;
        private FillFlowContainer slotFlow = null!;

        private bool selected;

        public PickerGroup(TournamentTeam team)
        {
            Team = team;

            // Taken here rather than in `load`, and each one kept in a field, because a bound copy is only
            // weakly held by the model it copies.
            roster = team.Players.GetBoundCopy();
            fullName = team.FullName.GetBoundCopy();
            acronym = team.Acronym.GetBoundCopy();

            Size = new Vector2(WIDTH, HEIGHT);
            Masking = true;
            CornerRadius = 4;
        }

        /// <summary>
        /// The positions currently displayed, in tier order.
        /// </summary>
        public IReadOnlyList<Drawable> TierSlots => slotFlow.Children;

        /// <summary>
        /// The name shown in the box's header.
        /// </summary>
        public string Title => string.IsNullOrEmpty(Team.FullName.Value) ? Team.Acronym.Value : Team.FullName.Value;

        /// <summary>
        /// Whether this box is the one the next draw has been pointed at, shown by the box taking the focused
        /// style so that where the next player will go can be seen at a glance.
        /// </summary>
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                    return;

                selected = value;
                updateBackgroundState();
            }
        }

        /// <summary>
        /// Reject what the box is showing, e.g. a draw which was asked for while this box had no room for it.
        /// </summary>
        public void FlashInputError() => background.FlashOnInputError();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                new GridContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Vertical = 5, Horizontal = 8 },
                    RowDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            new MarqueeContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                NonOverflowingContentAnchor = Anchor.Centre,
                                Margin = new MarginPadding { Vertical = 5 },
                                CreateContent = () => titleText = new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = OsuFont.Style.Heading1,
                                    Colour = colourProvider.Colour1,
                                    Text = Title,
                                },
                            },
                        ],
                        [
                            slotFlow = new FillFlowContainer
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, slot_spacing),
                            },
                        ],
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roster.BindCollectionChanged(onRosterChanged);

            // Built here rather than by the binding's initial pass, so that arriving at a selection which is
            // already under way does not announce every position as if it had just been filled.
            refreshSlots();

            // Those update methods shouldn't run immediately upon load complete,
            // since the `CreateContent()` for MarqueeContainer might not be invoked.
            fullName.BindValueChanged(_ => updateTitle());
            acronym.BindValueChanged(_ => updateTitle());
        }

        /// <summary>
        /// Re-read the positions. The roster binding covers a player being placed or handed back; this is for
        /// the player themselves changing — a profile which arrives after they were drawn.
        /// </summary>
        public void Refresh() => refreshSlots();

        /// <summary>
        /// Follow the roster, pointing out any front-row player which has just arrived: the box flashes and the
        /// position they landed in blinks, so a draw is shown where it happened rather than only being said.
        /// </summary>
        private void onRosterChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (TournamentUser player in e.NewItems)
                {
                    if (DraftInfo.IsFrontRow(player))
                        unannounced.Add(player);
                }
            }

            if (unannounced.Count > 0)
                background.FlashOnCommit();

            refreshSlots();
        }

        private void updateTitle()
        {
            titleText.Text = Title;
        }

        private void refreshSlots()
        {
            var slots = new List<Drawable>();

            for (int tier = 1; tier <= DraftInfo.FRONT_ROW_TIERS; tier++)
            {
                TournamentUser? player = DraftInfo.PlayerOfTier(Team, tier);

                slots.Add(new PickerPlayerCard
                {
                    RelativeSizeAxes = Axes.X,
                    Height = SLOT_HEIGHT,
                    Tier = tier,
                    Player = player,
                });
            }

            unannounced.Clear();

            slotFlow.Children = slots;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button != MouseButton.Left)
                return base.OnClick(e);

            OnClicked?.Invoke(this);
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateBackgroundState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateBackgroundState();
            base.OnHoverLost(e);
        }

        private void updateBackgroundState()
            => background.VisualStyle = selected ? VisualStyle.Focused : IsHovered ? VisualStyle.Hovered : VisualStyle.Normal;
    }
}
