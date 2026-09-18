// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// One position of a draft group: the player standing in it, or the fact that it is still waiting for one.
    /// </summary>
    public partial class PlayerSlotCard : Container
    {
        /// <summary>
        /// The height the ratios below are written against, i.e. the height of a draft group's position.
        /// </summary>
        public const float REFERENCE_HEIGHT = 24;

        private const float badge_width_ratio = 20 / REFERENCE_HEIGHT;
        private const float badge_size_ratio = 12 / REFERENCE_HEIGHT;
        private const float icon_size_ratio = 13 / REFERENCE_HEIGHT;
        private const float name_size_ratio = 14 / REFERENCE_HEIGHT;
        private const float name_margin_ratio = 5 / REFERENCE_HEIGHT;

        /// <summary>
        /// The tier the position stands for. Only read while the position is empty: one holding a player says
        /// which tier that player is by their own badge.
        /// </summary>
        public int Tier
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                rebuild();
            }
        }

        /// <summary>
        /// The player this position holds, or <c>null</c> while it is empty.
        /// </summary>
        public TournamentUser? Player
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                rebuild();
            }
        }

        /// <summary>
        /// Invoked when a filled position is right-clicked, or <c>null</c> to leave the card read-only — clicks
        /// then pass straight through to whatever holds it.
        /// </summary>
        public Action<TournamentUser>? OnRemove { get; set; }

        /// <summary>
        /// The size the card takes, which everything it draws is scaled from.
        /// </summary>
        public override Vector2 Size
        {
            get => base.Size;
            set
            {
                if (base.Size == value)
                    return;

                base.Size = value;
                rebuild();
            }
        }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public PlayerSlotCard()
        {
            // Masked because the marquee scrolls a name wider than the position, and because a position which is
            // exactly as wide as the line it sits on must not let anything spill over the one beside it.
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load() => rebuild();

        protected override bool OnClick(ClickEvent e)
        {
            if (Player == null || OnRemove == null || e.Button != MouseButton.Right)
                return base.OnClick(e);

            // The front row is the random phase's to place, so right-clicking one of those does nothing at all.
            if (!DraftInfo.IsFrontRow(Player))
                OnRemove(Player);

            return true;
        }

        /// <summary>
        /// Build what the current properties describe, replacing whatever was there.
        /// </summary>
        private void rebuild()
        {
            if (LoadState == LoadState.NotLoaded)
                return;

            float badgeWidth = Size.Y * badge_width_ratio;
            float nameMargin = Size.Y * name_margin_ratio;
            float nameSize = Size.Y * name_size_ratio;

            // A position's badge says what it stands for: the tier of the player in it, an empty tier's own
            // number, or a member's lack of either.
            int badgeTier = Player == null
                ? Tier
                : DraftInfo.IsFrontRow(Player)
                    ? Player.Tier
                    : DraftInfo.NO_TIER;

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
                        createBadge(badgeTier, badgeWidth, Size.Y, dimmed: Player == null),
                        createContent(nameSize, nameMargin),
                    ],
                },
            };
        }

        /// <summary>
        /// The name of the player in this position, or the fact that it is still waiting for one.
        /// </summary>
        private Drawable createContent(float nameSize, float nameMargin)
        {
            if (Player == null)
            {
                return new TournamentSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = nameMargin },
                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: nameSize),
                    Colour = colourProvider.Background1,
                    Text = DraftStrings.EmptySlot,
                };
            }

            TournamentUser shown = Player;

            return new MarqueeContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                NonOverflowingContentAnchor = Anchor.CentreLeft,
                Margin = new MarginPadding { Left = nameMargin },
                CreateContent = () => new TournamentSpriteText
                {
                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: nameSize),
                    Shadow = false,
                    Text = displayName(shown),
                },
            };
        }

        private static string displayName(TournamentUser player)
            => string.IsNullOrEmpty(player.Username) ? $@"#{player.OnlineID}" : player.Username;

        /// <summary>
        /// The badge which marks what a position is, so that an empty one still says what it is waiting for.
        /// </summary>
        private Container createBadge(int tier, float width, float height, bool dimmed) => new Container
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Size = new Vector2(width, height),
            Child = tier == DraftInfo.NO_TIER
                ? new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(height * icon_size_ratio),
                    Colour = dimmed ? colourProvider.Background1 : colourProvider.Content1,
                    Icon = FontAwesome.Solid.User,
                }
                : new TournamentSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = $@"T{tier}",
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: height * badge_size_ratio),
                    Colour = dimmed ? colourProvider.Background1 : colourProvider.Colour1,
                },
        };
    }
}
