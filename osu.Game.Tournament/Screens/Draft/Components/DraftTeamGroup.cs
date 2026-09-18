// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;
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
        private const float vertical_margin = 8;
        private const float row_height = 24;
        private const float row_spacing = 3;
        private const float cell_spacing = 6;
        private const float member_top = 34;

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

        /// <summary>
        /// The draft's per-team member limit, held as a copy rather than as the setting itself so that a change
        /// to it is reflected without the screen rebuilding every group.
        /// </summary>
        private readonly Bindable<int> max;

        private readonly BindableList<TournamentUser> roster;
        private readonly Bindable<string> fullName;
        private readonly Bindable<string> acronym;

        private FormControlBackground background = null!;
        private TournamentSpriteText titleText = null!;
        private TournamentSpriteText countText = null!;
        private FillFlowContainer slotFlow = null!;

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
        public string Title => string.IsNullOrEmpty(Team.FullName.Value) ? Team.Acronym.Value : Team.FullName.Value;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Masking = true,
                    Padding = new MarginPadding { Horizontal = horizontal_margin, Vertical = vertical_margin },
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                            ],
                            ColumnDimensions =
                            [
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
                            ],
                            Content = new Drawable[][]
                            {
                                [
                                    new MarqueeContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        NonOverflowingContentAnchor = Anchor.CentreLeft,
                                        CreateContent = () => titleText = new TournamentSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 15),
                                            Colour = colourProvider.Colour1,
                                            Text = Title,
                                        },
                                    },
                                    countText = new TournamentSpriteText
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 13),
                                        Colour = colourProvider.Foreground1,
                                    },
                                ],
                            },
                        },
                        slotFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Full,
                            Spacing = new Vector2(cell_spacing, row_spacing),
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // The member limit has to be in place before the roster binding below fires its immediate update.
            max.BindValueChanged(_ => refreshSlots());

            roster.BindCollectionChanged((_, _) => refreshSlots(), true);

            fullName.BindValueChanged(_ => updateTitle());
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
                cells.Add(createCell(tier, DraftInfo.PlayerOfTier(Team, tier)));
                tiers.Add(tier);
            }

            for (int i = 0; i < memberSlots; i++)
            {
                cells.Add(createCell(DraftInfo.NO_TIER, i < members.Count ? members[i] : null));
                tiers.Add(DraftInfo.NO_TIER);
            }

            MemberSlots = cells;
            SlotTiers = tiers;
            slotFlow.Children = cells;

            // The box follows the number of lines the limit calls for rather than holding a fixed height, which
            // is what makes sharing a line pay off: a roster of six costs three lines instead of four.
            int rows = (cells.Count + cells_per_row - 1) / cells_per_row;
            Height = Math.Max(min_height, member_top + rows * row_height + Math.Max(0, rows - 1) * row_spacing + vertical_margin);
            countText.Text = $@"{Team.Players.Count}/{limit}";
        }

        private void updateBackgroundState()
            => background.VisualStyle = IsHovered ? VisualStyle.Hovered : VisualStyle.Normal;

        private void removeMember(TournamentUser player) => OnMemberRemovalRequested?.Invoke(this, player);

        /// <summary>
        /// One position of this group, sized to the grid the box laid out for it.
        /// </summary>
        private PlayerSlotCard createCell(int tier, TournamentUser? player)
            => new PlayerSlotCard
            {
                Size = new Vector2(cell_width, row_height),
                Tier = tier,
                Player = player,
                OnRemove = removeMember,
            };
    }
}
