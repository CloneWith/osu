// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.RandomPick.Components
{
    /// <summary>
    /// One tier position of a random pick box: the player the team has drawn from that tier, shown as their
    /// avatar, name and rank, or the fact that the tier is still waiting for one.
    /// </summary>
    public partial class PickerPlayerCard : Container
    {
        /// <summary>
        /// The height the ratios below are written against, i.e. the height of the panel this card is laid out
        /// like.
        /// </summary>
        private const float reference_height = 56;

        private const float padding_ratio = 6 / reference_height;
        private const float avatar_size_ratio = 44 / reference_height;
        private const float avatar_corner_ratio = 6 / reference_height;
        private const float text_margin_ratio = 10 / reference_height;
        private const float row_spacing_ratio = 1 / reference_height;
        private const float name_size_ratio = 24 / reference_height;
        private const float rank_size_ratio = 16 / reference_height;

        /// <summary>
        /// How much of itself a position with nobody in it shows its placeholder avatar at, so that an empty one
        /// does not read as a player nobody has heard of.
        /// </summary>
        private const float empty_avatar_alpha = 0.35f;

        /// <summary>
        /// The tier this position stands for. It is named on the card while the position is empty, so that what
        /// is still to be drawn for can be read off the box.
        /// </summary>
        public int Tier { get; init; }

        /// <summary>
        /// The player drawn into this position, or <c>null</c> while the tier has not been drawn for.
        /// </summary>
        public TournamentUser? Player { get; init; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public PickerPlayerCard()
        {
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            float padding = Size.Y * padding_ratio;
            float avatarSize = Size.Y * avatar_size_ratio;
            float textMargin = Size.Y * text_margin_ratio;
            float rowSpacing = Size.Y * row_spacing_ratio;

            TournamentUser? drawn = Player;

            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(padding),
                ColumnDimensions =
                [
                    new Dimension(GridSizeMode.AutoSize),
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
                            Size = new Vector2(avatarSize),
                            Masking = true,
                            CornerRadius = Size.Y * avatar_corner_ratio,
                            Alpha = drawn == null ? empty_avatar_alpha : 1,
                            Child = new TournamentAvatar(drawn?.ToAPIUser())
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = $@"T{Tier}",
                            Colour = drawn != null ? colourProvider.Colour1 : colourProvider.Background1,
                            Margin = new MarginPadding { Horizontal = textMargin },
                            Font = OsuFont.Style.Heading2,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(rowSpacing),
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new MarqueeContainer
                                {
                                    NonOverflowingContentAnchor = Anchor.CentreLeft,
                                    CreateContent = () => new TournamentSpriteText
                                    {
                                        Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: Size.Y * name_size_ratio),
                                        Colour = drawn != null ? colourProvider.Content1 : colourProvider.Background1,
                                        Text = drawn != null ? displayName(drawn) : DraftStrings.EmptySlot,
                                    },
                                },
                                new TournamentSpriteText
                                {
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: Size.Y * rank_size_ratio),
                                    Colour = drawn != null ? colourProvider.Foreground1 : colourProvider.Background1,
                                    Text = drawn != null ? rankOf(drawn) : RandomPickStrings.PoolHeader(Tier),
                                },
                            },
                        },
                    ],
                },
            };
        }

        /// <summary>
        /// The rank of the player in this position, or a dash while their profile has not been fetched.
        /// </summary>
        private static LocalisableString rankOf(TournamentUser player) => player.Rank?.ToLocalisableString("\\##,##0") ?? "-";

        private static string displayName(TournamentUser player)
            => string.IsNullOrEmpty(player.Username) ? $@"#{player.OnlineID}" : player.Username;
    }
}
