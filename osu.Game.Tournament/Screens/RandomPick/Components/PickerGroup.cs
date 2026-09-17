// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.RandomPick.Components
{
    /// <summary>
    /// One team's box on the random pick screen: the team, and the player it has drawn from each tier.
    /// </summary>
    public partial class PickerGroup : Container
    {
        public const float WIDTH = 210;

        /// <summary>
        /// The height of the box. It is fixed because the boxes are laid out in two rows which grow towards the
        /// middle of the screen, so they all have to agree on how tall they are.
        /// </summary>
        public const float HEIGHT = 118;

        private const float horizontal_margin = 6;
        private const float slot_height = 28;
        private const float slot_spacing = 3;
        private const float slot_top = 22;

        private static readonly Color4 title_colour = new Color4(255, 204, 34, 255);

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

        private readonly FormControlBackground background;
        private readonly TournamentSpriteText titleText;
        private readonly FillFlowContainer slotFlow;

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

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                titleText = new TournamentSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Position = new Vector2(0, 5),
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 12),
                    Colour = title_colour,
                },
                slotFlow = new FillFlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(horizontal_margin, slot_top),
                    AutoSizeAxes = Axes.Y,
                    Width = WIDTH - horizontal_margin * 2,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, slot_spacing),
                },
            };
        }

        /// <summary>
        /// The tier positions currently displayed, in tier order.
        /// </summary>
        public IReadOnlyList<Drawable> TierSlots { get; private set; } = [];

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
        private void load()
        {
            roster.BindCollectionChanged(onRosterChanged);

            // Built here rather than by the binding's initial pass, so that arriving at a selection which is
            // already under way does not announce every position as if it had just been filled.
            refreshSlots();

            fullName.BindValueChanged(_ => updateTitle(), true);
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
            => titleText.Text = string.IsNullOrEmpty(Team.FullName.Value) ? Team.Acronym.Value : Team.FullName.Value;

        private void refreshSlots()
        {
            var slots = new List<Drawable>();

            for (int tier = 1; tier <= DraftInfo.FRONT_ROW_TIERS; tier++)
            {
                TournamentUser? player = DraftInfo.PlayerOfTier(Team, tier);

                slots.Add(player != null
                    ? new PickerPlayerCell(tier, player, unannounced.Contains(player))
                    : new PickerEmptySlot(tier));
            }

            unannounced.Clear();

            TierSlots = slots;
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

        private static Color4 tierColourFor(int tier) => tier switch
        {
            1 => FumoColours.SunshineYellow.Light,
            2 => FumoColours.SeaBlue.Light,
            3 => FumoColours.LightGreen.Light,
            _ => OsuColour.Gray(0.8f),
        };

        /// <summary>
        /// The tier badge, so a position says which tier it stands for whether or not it has a player in it.
        /// </summary>
        private static Container createBadge(int tier, bool dimmed) => new Container
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Size = new Vector2(20, slot_height),
            Child = new TournamentSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = $@"T{tier}",
                Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 12),
                Colour = dimmed ? OsuColour.Gray(0.35f) : tierColourFor(tier),
            },
        };

        private static string displayName(TournamentUser player)
            => string.IsNullOrEmpty(player.Username) ? $@"#{player.OnlineID}" : player.Username;

        /// <summary>
        /// One player the random phase drew into this team, shown as their avatar.
        /// </summary>
        public partial class PickerPlayerCell : Container
        {
            private const double flash_duration = 180;
            private const double flash_hold = 140;

            /// <summary>
            /// The colour a newly placed player's name blinks in, before settling back to normal.
            /// </summary>
            private static readonly Color4 announcement_colour = FumoColours.LightGreen.Lighter;

            /// <summary>
            /// The player this row shows.
            /// </summary>
            public readonly TournamentUser Player;

            /// <param name="tier">The tier of the position this row stands for.</param>
            /// <param name="player">The player to display.</param>
            /// <param name="announce">Whether the row has just been filled, i.e. whether it should blink.</param>
            public PickerPlayerCell(int tier, TournamentUser player, bool announce)
            {
                Player = player;

                Size = new Vector2(WIDTH - horizontal_margin * 2, slot_height);
                Masking = true;

                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            createBadge(tier, false),
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(slot_height - 2),
                                Masking = true,
                                CornerRadius = 4,
                                Margin = new MarginPadding { Left = 2, Right = 5 },
                                Child = new TournamentAvatar(player)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            },
                            new MarqueeContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                NonOverflowingContentAnchor = Anchor.CentreLeft,
                                CreateContent = () =>
                                {
                                    var name = new TournamentSpriteText
                                    {
                                        Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 13),
                                        Shadow = false,
                                        Text = displayName(player),
                                    };

                                    if (announce)
                                        blink(name);

                                    return name;
                                },
                            },
                        ],
                    },
                };
            }

            /// <summary>
            /// Blink the name a couple of times, in the colour a placement is announced in.
            /// </summary>
            /// <remarks>
            /// The colour to settle back to is read off the text rather than assumed, because the transforms
            /// start before it is on screen and nothing here knows what the theme left it as.
            /// </remarks>
            private static void blink(TournamentSpriteText name)
            {
                ColourInfo idle = name.Colour;

                name.FadeColour(announcement_colour, flash_duration, Easing.OutQuint)
                    .Then().Delay(flash_hold).FadeColour(idle, flash_duration, Easing.OutQuint)
                    .Then().Delay(flash_hold).FadeColour(announcement_colour, flash_duration, Easing.OutQuint)
                    .Then().Delay(flash_hold).FadeColour(idle, flash_duration, Easing.OutQuint);
            }
        }

        /// <summary>
        /// A tier this team has not drawn yet.
        /// </summary>
        private partial class PickerEmptySlot : Container
        {
            public PickerEmptySlot(int tier)
            {
                Size = new Vector2(WIDTH - horizontal_margin * 2, slot_height);

                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            createBadge(tier, true),
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 5 },
                                Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 13),
                                Colour = OsuColour.Gray(0.35f),
                                Text = @"—",
                            },
                        ],
                    },
                };
            }
        }
    }
}
