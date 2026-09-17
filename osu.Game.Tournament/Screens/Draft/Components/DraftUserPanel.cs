// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Draft.Components
{
    /// <summary>
    /// A single player in the draft pool, laid out like <see cref="UserPanel"/> so it stays visually
    /// consistent with the other player lists in the tournament client.
    /// </summary>
    public partial class DraftUserPanel : UserPanel, IHasContextMenu
    {
        public const float WIDTH = 200;
        public const float HEIGHT = 56;

        /// <summary>
        /// The tournament user this panel represents.
        /// </summary>
        public readonly TournamentUser Player;

        /// <summary>
        /// Raised when this panel is left-clicked.
        /// </summary>
        public event Action<DraftUserPanel>? OnSelected;

        /// <summary>
        /// Whether this panel is the one the next placement will use.
        /// </summary>
        public BindableBool Selected { get; } = new BindableBool();

        private FormControlBackground background = null!;
        private MarqueeContainer usernameMarquee = null!;
        private TournamentSpriteText rankText = null!;

        public DraftUserPanel(TournamentUser player)
            : base(player.ToAPIUser())
        {
            Player = player;

            Size = new Vector2(WIDTH, HEIGHT);
            CornerRadius = 5;
        }

        /// <inheritdoc cref="UserPanel.ContextMenuItems"/>
        /// <remarks>
        /// Intentionally empty — see the class remarks.
        /// </remarks>
        MenuItem[] IHasContextMenu.ContextMenuItems => [];

        // The cover background would kick off an image download per panel; a flat colour is both cheaper and
        // more legible for a list that is meant to be scanned quickly.
        protected override Drawable? CreateBackground() => null;

        protected override Drawable CreateLayout()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new FormControlBackground(),
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(6),
                        ColumnDimensions =
                        [
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                        ],
                        Content = new Drawable[][]
                        {
                            [
                                new Container
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Size = new Vector2(44),
                                    Masking = true,
                                    CornerRadius = 6,
                                    Children = new Drawable[]
                                    {
                                        new TournamentAvatar(User)
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                    },
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(1),
                                    Margin = new MarginPadding { Left = 10 },
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        usernameMarquee = new MarqueeContainer
                                        {
                                            NonOverflowingContentAnchor = Anchor.CentreLeft,
                                            CreateContent = createUsernameText,
                                        },
                                        rankText = new TournamentSpriteText
                                        {
                                            Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 15),
                                            Colour = OsuColour.Gray(0.75f),
                                        },
                                    },
                                },
                            ],
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Selected.BindValueChanged(_ => updateState(), true);

            Refresh();
        }

        /// <summary>
        /// Re-read the displayed information from <see cref="Player"/>, e.g. after its profile was fetched.
        /// </summary>
        public void Refresh()
        {
            // Handing the marquee its factory again is how it is told the displayed name has changed: it
            // rebuilds its scrolling copies and measures afresh whether there is any overflow to scroll.
            usernameMarquee.CreateContent = createUsernameText;

            rankText.Text = Player.Rank?.ToLocalisableString("\\##,##0") ?? "-";
        }

        private TournamentSpriteText createUsernameText() => new TournamentSpriteText
        {
            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 22),
            Shadow = false,
            Text = string.IsNullOrEmpty(Player.Username) ? $@"#{Player.OnlineID}" : Player.Username,
        };

        private void updateState()
        {
            if (Selected.Value)
                background.VisualStyle = VisualStyle.Focused;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        protected override bool OnClick(ClickEvent e)
        {
            // Left click selects instead of opening the profile overlay, which must never appear on stream.
            if (e.Button != MouseButton.Left)
                return base.OnClick(e);

            OnSelected?.Invoke(this);
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
    }
}
