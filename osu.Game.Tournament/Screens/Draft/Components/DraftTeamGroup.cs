// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Draft.Components
{
    /// <summary>
    /// One draft group, showing the team it will eventually become together with the players placed into it.
    /// </summary>
    public partial class DraftTeamGroup : Container
    {
        public const float WIDTH = 240;

        /// <summary>
        /// The height of the shortest box, holding nothing but the front row.
        /// </summary>
        private const float min_height = 70;

        /// <summary>How many positions share a line.</summary>
        private const int cells_per_row = 2;

        private const float horizontal_margin = 8;
        private const float row_height = 24;
        private const float row_spacing = 3;
        private const float cell_spacing = 6;
        private const float member_top = 34;
        private const float bottom_padding = 8;

        /// <summary>
        /// A little less than an exact fit, so that a rounding error cannot push a second cell onto a line of
        /// its own — a cell is laid out by the width it was given, and the listing decides where to wrap.
        /// </summary>
        private const float fit_slack = 0.5f;

        /// <summary>
        /// The width of one position. <see cref="cells_per_row"/> of them plus the spacing between them fill the
        /// box exactly, so every cell of every group lines up.
        /// </summary>
        private const float cell_width = (WIDTH - horizontal_margin * 2 - cell_spacing * (cells_per_row - 1)) / cells_per_row - fit_slack;

        /// <summary>
        /// The team this group represents. Note that draft groups are independent of the bracket teams.
        /// </summary>
        public readonly TournamentTeam Team;

        /// <summary>
        /// Raised when this group is left-clicked, i.e. the currently selected pool player should be placed in
        /// its next free member position.
        /// </summary>
        public event Action<DraftTeamGroup>? OnClicked;

        /// <summary>
        /// Raised when a drafted member of this group is right-clicked.
        /// </summary>
        public event Action<DraftTeamGroup, TournamentUser>? OnMemberRemovalRequested;

        private static readonly Color4 title_colour = new Color4(255, 204, 34, 255);

        /// <summary>
        /// The draft's per-team member limit, held as a copy rather than as the setting itself so that a change
        /// to it is reflected without the screen rebuilding every group.
        /// </summary>
        private readonly Bindable<int> max;

        private readonly BindableList<TournamentUser> roster;
        private readonly Bindable<string> fullName;
        private readonly Bindable<string> acronym;

        private readonly FormControlBackground background;
        private readonly TournamentSpriteText titleText;
        private readonly TournamentSpriteText countText;
        private readonly FillFlowContainer slotFlow;

        public DraftTeamGroup(TournamentTeam team, Bindable<int> maxMembers)
        {
            Team = team;

            // Taken here rather than in `load` so that the group never holds the setting itself.
            max = maxMembers.GetBoundCopy();

            // Likewise taken here, but because a bound copy has to be held by somebody.
            roster = team.Players.GetBoundCopy();
            fullName = team.FullName.GetBoundCopy();
            acronym = team.Acronym.GetBoundCopy();

            Size = new Vector2(WIDTH, min_height);
            Masking = true;
            CornerRadius = 6;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                titleText = new TournamentSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(horizontal_margin, 6),
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 15),
                    Colour = title_colour,
                },
                countText = new TournamentSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Position = new Vector2(-horizontal_margin, 7),
                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 13),
                    Colour = OsuColour.Gray(0.7f),
                },
                new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = 28,
                    Size = new Vector2(WIDTH - horizontal_margin * 2, 1.5f),
                    Colour = Color4.Black.Opacity(0.5f),
                },
                slotFlow = new FillFlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(horizontal_margin, member_top),
                    AutoSizeAxes = Axes.Y,
                    Width = WIDTH - horizontal_margin * 2,
                    Direction = FillDirection.Full,
                    Spacing = new Vector2(cell_spacing, row_spacing),
                },
            };
        }

        /// <summary>
        /// The positions currently displayed, in layout order.
        /// </summary>
        public IReadOnlyList<Drawable> MemberSlots { get; private set; } = [];

        /// <summary>
        /// The tier each position in <see cref="MemberSlots"/> stands for, in the same order —
        /// <see cref="DraftInfo.NO_TIER"/> for a member position.
        /// </summary>
        public IReadOnlyList<int> SlotTiers { get; private set; } = [];

        /// <summary>
        /// The name shown in the box's header.
        /// </summary>
        public string Title { get; private set; } = string.Empty;

        [BackgroundDependencyLoader]
        private void load()
        {
            // The member limit has to be in place before the roster binding below fires its immediate update.
            max.BindValueChanged(_ => refreshSlots());

            roster.BindCollectionChanged((_, _) => refreshSlots(), true);

            fullName.BindValueChanged(_ => updateTitle(), true);
            acronym.BindValueChanged(_ => updateTitle());
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button != MouseButton.Left)
                return base.OnClick(e);

            OnClicked?.Invoke(this);
            background.FlashOnCommit();
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

        private void updateTitle()
        {
            Title = string.IsNullOrEmpty(Team.FullName.Value) ? Team.Acronym.Value : Team.FullName.Value;
            titleText.Text = Title;
        }

        /// <summary>
        /// Rebuild the listing from <see cref="Team"/>. Every group shows one position per tier plus as many
        /// member positions as the limit leaves room for, empty ones included, so a group being filled up never
        /// changes shape.
        /// </summary>
        private void refreshSlots()
        {
            int limit = max.Value;
            int tierSlots = Math.Min(DraftInfo.FRONT_ROW_TIERS, limit);
            int memberSlots = Math.Max(0, limit - DraftInfo.FRONT_ROW_TIERS);

            // The member positions are filled densely, so the third position onwards is simply the member list.
            // The front row is read from the tiers instead, which is what lets an undrawn tier show as empty.
            List<TournamentUser> members = DraftInfo.DraftedMembersOf(Team).ToList();

            var cells = new List<Drawable>();
            var tiers = new List<int>();

            for (int tier = 1; tier <= tierSlots; tier++)
            {
                TournamentUser? player = DraftInfo.PlayerOfTier(Team, tier);

                cells.Add(player != null
                    ? new DraftMemberCell(player, cell_width, removeMember)
                    : new DraftEmptySlot(tier, cell_width));

                tiers.Add(tier);
            }

            for (int i = 0; i < memberSlots; i++)
            {
                cells.Add(i < members.Count
                    ? new DraftMemberCell(members[i], cell_width, removeMember)
                    : new DraftEmptySlot(DraftInfo.NO_TIER, cell_width));

                tiers.Add(DraftInfo.NO_TIER);
            }

            MemberSlots = cells;
            SlotTiers = tiers;
            slotFlow.Children = cells;

            // The box follows the number of lines the limit calls for rather than holding a fixed height, which
            // is what makes sharing a line pay off: a roster of six costs three lines instead of four.
            int rows = (cells.Count + cells_per_row - 1) / cells_per_row;
            Height = Math.Max(min_height, member_top + rows * row_height + Math.Max(0, rows - 1) * row_spacing + bottom_padding);
            countText.Text = $@"{Team.Players.Count}/{limit}";
        }

        private void updateBackgroundState()
            => background.VisualStyle = IsHovered ? VisualStyle.Hovered : VisualStyle.Normal;

        private void removeMember(TournamentUser player) => OnMemberRemovalRequested?.Invoke(this, player);

        private static Color4 tierColourFor(int tier) => tier switch
        {
            1 => FumoColours.SunshineYellow.Light,
            2 => FumoColours.SeaBlue.Light,
            3 => FumoColours.LightGreen.Light,
            _ => OsuColour.Gray(0.8f),
        };

        /// <summary>
        /// The badge which marks what a cell is, so an empty one still says what it is waiting for.
        /// </summary>
        private static Container createBadge(int tier, float width, bool dimmed) => new Container
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Size = new Vector2(width, row_height),
            Child = tier == DraftInfo.NO_TIER
                ? new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Colour = dimmed ? OsuColour.Gray(0.35f) : OsuColour.Gray(0.8f),
                    Icon = FontAwesome.Solid.User,
                }
                : new TournamentSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = $@"T{tier}",
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 12),
                    Colour = dimmed ? OsuColour.Gray(0.35f) : tierColourFor(tier),
                },
        };

        private const float badge_width = 20;

        /// <summary>
        /// One player in an occupied position, right-clickable to send them back to the pool unless the random
        /// phase is what put them there.
        /// </summary>
        private partial class DraftMemberCell : Container
        {
            private readonly TournamentUser player;
            private readonly Action<TournamentUser> onRemove;

            /// <param name="player">The player to display.</param>
            /// <param name="width">The width of the position within the box.</param>
            /// <param name="onRemove">Invoked when the cell is right-clicked.</param>
            public DraftMemberCell(TournamentUser player, float width, Action<TournamentUser> onRemove)
            {
                this.player = player;
                this.onRemove = onRemove;

                Size = new Vector2(width, row_height);

                // The marquee scrolls a name wider than the cell, so the cell clips it.
                Masking = true;

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
                            createBadge(DraftInfo.IsFrontRow(player) ? player.Tier : DraftInfo.NO_TIER, badge_width, false),
                            new MarqueeContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                NonOverflowingContentAnchor = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 5 },
                                CreateContent = () => new TournamentSpriteText
                                {
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14),
                                    Shadow = false,
                                    Text = displayName,
                                },
                            },
                        ],
                    },
                };
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (e.Button != MouseButton.Right)
                    return base.OnClick(e);

                // The front row is the random phase's to place, so right-clicking one of them does nothing at all.
                if (!DraftInfo.IsFrontRow(player))
                    onRemove(player);

                return true;
            }

            private string displayName => string.IsNullOrEmpty(player.Username) ? $@"#{player.OnlineID}" : player.Username;
        }

        /// <summary>
        /// An empty position, showing which tier it stands for or that it takes a drafted member.
        /// </summary>
        private partial class DraftEmptySlot : Container
        {
            /// <param name="tier">The tier of the position this slot stands for, or <see cref="DraftInfo.NO_TIER"/>.</param>
            /// <param name="width">The width of the position within the box.</param>
            public DraftEmptySlot(int tier, float width)
            {
                Size = new Vector2(width, row_height);

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
                            createBadge(tier, badge_width, true),
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 5 },
                                Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 14),
                                Colour = OsuColour.Gray(0.35f),
                                Text = DraftStrings.EmptySlot,
                            },
                        ],
                    },
                };
            }
        }
    }
}
