// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A stateless beatmap card for presentation purposes.
    /// </summary>
    public partial class FumoBeatmapCard : CompositeDrawable
    {
        private const float height = 80;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        /// <summary>
        /// The relevant <see cref="RoundBeatmap"/> of the beatmap panel.
        /// </summary>
        public readonly RoundBeatmap Beatmap;

        private StarRatingDisplay starRatingDisplay = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public FumoBeatmapCard(RoundBeatmap beatmap)
        {
            Beatmap = beatmap;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindTo(ladder.CurrentMatch);

            InternalChildren = new Drawable[]
            {
                new EmptyBox(5)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    BoxColour = Color4.White,
                },
                new Container
                {
                    Name = @"Beatmap information",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Masking = true,
                    CornerRadius = 5,
                    Children = new Drawable[]
                    {
                        new NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            OnlineInfo = Beatmap.Beatmap,
                        },
                        new Box
                        {
                            Name = @"Background addition",
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.2f,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding { Vertical = 5, Horizontal = 8 },
                            RowDimensions =
                            [
                                new Dimension(),
                            ],
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            ],
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FumoChessIcon(Beatmap.Mods, Beatmap.ModIndex)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(40),
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(2),
                                        Padding = new MarginPadding { Left = 8, Right = 5 },
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new MarqueeContainer
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                NonOverflowingContentAnchor = Anchor.CentreLeft,
                                                CreateContent = () => new OsuSpriteText
                                                {
                                                    Font = OsuFont.Style.Heading1,
                                                    Text = Beatmap.Beatmap?.Metadata.TitleUnicode ?? "Unknown title",
                                                },
                                            },
                                            new MarqueeContainer
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                NonOverflowingContentAnchor = Anchor.CentreLeft,
                                                CreateContent = () => new OsuSpriteText
                                                {
                                                    Font = OsuFont.Style.Heading2,
                                                    Text = Beatmap.Beatmap?.Metadata.ArtistUnicode ?? "Unknown artist",
                                                },
                                            },
                                            new MarqueeContainer
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                NonOverflowingContentAnchor = Anchor.CentreLeft,
                                                CreateContent = () => new FillFlowContainer
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(3),
                                                    Children = new Drawable[]
                                                    {
                                                        starRatingDisplay = new StarRatingDisplay(starDifficulty: new StarDifficulty(Beatmap.StarRatingWithMod ?? Beatmap.Beatmap?.StarRating ?? 0,
                                                            Beatmap.MaxCombo), StarRatingDisplaySize.Small, true)
                                                        {
                                                            Name = @"Star rating pill",
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                        },
                                                        new OsuSpriteText
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Font = OsuFont.Style.Body,
                                                            Text = Beatmap.DifficultyField,
                                                        },
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Beatmap.StarRatingWithMod == null && Beatmap.Beatmap != null && TournamentExtensions.SPECIAL_MODS.Contains(Beatmap.Mods))
            {
                var request = new GetBeatmapAttributesRequest(Beatmap.Beatmap.OnlineID, TournamentExtensions.ToModEnum(Beatmap.Mods));

                request.Success += result =>
                {
                    Beatmap.StarRatingWithMod = Math.Round(result.Attributes.StarRating, 2);
                    Beatmap.MaxCombo = result.Attributes.MaxCombo;
                    starRatingDisplay.Current.Value = new StarDifficulty(Beatmap.StarRatingWithMod.Value, Beatmap.MaxCombo);
                };

                api.Queue(request);
            }
        }
    }
}
