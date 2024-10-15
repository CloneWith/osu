// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableMapCard : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        public const float WIDTH = 550;

        public const float HEIGHT = 75;

        private readonly string mod;
        private readonly string index;

        private Box colorBackground = null!;
        private SpriteIcon statusIcon = null!;
        private TournamentModIcon? modIcon;
        private Container beatmapInfoContainer = null!;
        private FillFlowContainer infoContainer = null!;
        private Box topMask = null!;
        private Box backgroundAddition = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public DrawableMapCard(IBeatmapInfo? map, string mod = "", string index = "")
        {
            Beatmap = map;
            this.mod = mod;
            this.index = index;
            Width = WIDTH;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            var displayTitle = Beatmap?.GetDisplayTitleRomanisable(false, false) ?? (LocalisableString)@"unknown";

            string[] songNameList = displayTitle.ToString().Split(' ');

            int firstHyphenIndex = 0;

            // Find the first " - " (Hopefully it isn't in the Artists field)
            for (int i = 0; i < songNameList.Count(); i++)
            {
                string obj = songNameList.ElementAt(i);

                if (obj == "-")
                {
                    firstHyphenIndex = i;
                    break;
                }
            }

            var titleList = songNameList.Skip(firstHyphenIndex + 1);

            // Re-construct
            string songName = string.Empty;

            for (int i = 0; i < titleList.Count(); i++)
            {
                songName += titleList.ElementAt(i).Trim();
                if (i != titleList.Count() - 1) songName += ' ';
            }

            string truncatedSongName = songName.Trim().TruncateWithEllipsis(39);

            string displayDifficulty = Beatmap?.DifficultyName ?? "unknown";
            string truncatedDifficultyName = displayDifficulty.TruncateWithEllipsis(25);

            Masking = true;
            // RelativeSizeAxes = Axes.Both;
            Shear = new Vector2(OsuGame.SHEAR, 0f);
            CornerRadius = 10f;

            InternalChildren = new Drawable[]
            {
                colorBackground = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientHorizontal(new Color4(1f, 1f, 1f, 0.4f), new Color4(0f, 0f, 0f, 0f)),
                },
                statusIcon = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.Heart,
                    Size = new Vector2(24),
                    Colour = Color4.White,
                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                    // Alpha = 0,
                },
                beatmapInfoContainer = new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.98f,
                    Height = 0.95f,
                    CornerRadius = 10f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = OsuColour.Gray(0.5f),
                            OnlineInfo = (Beatmap as IBeatmapSetOnlineInfo),
                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                            Scale = new Vector2(1.25f),
                        },
                        backgroundAddition = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.1f
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Padding = new MarginPadding(15),
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = truncatedSongName,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 32),
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new osuTK.Vector2(5),
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new osuTK.Vector2(5),
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Children = new Drawable[]
                                            {
                                                new DifficultyIcon(Beatmap, ladder.Ruleset.Value)
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    TooltipType = DifficultyIconTooltipType.None,
                                                    Scale = new Vector2(0.9f),
                                                },
                                                new StarRatingDisplay(starDifficulty: new StarDifficulty(Beatmap?.StarRating ?? 0, 0), animated: true)
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                                new TournamentSpriteText
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Text = truncatedDifficultyName,
                                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 23)
                                                },
                                            }
                                        },
                                        new TournamentSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Text = $"mapper: {Beatmap?.Metadata.Author}",
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Font = OsuFont.Torus.With(size: 18)
                                        }
                                    }
                                }
                            },
                        },
                        modIcon = new TournamentModIcon(index.IsNull() ? mod : mod + index)
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Margin = new MarginPadding { Right = 20 },
                            Size = new osuTK.Vector2(96),
                            RelativeSizeAxes = Axes.Y,
                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                        },
                        topMask = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Alpha = 0,
                        },
                    },
                },
            };
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
                match.OldValue.EXPicks.CollectionChanged -= picksBansOnCollectionChanged;
            if (match.NewValue != null)
                match.NewValue.EXPicks.CollectionChanged += picksBansOnCollectionChanged;

            Scheduler.AddOnce(updateState);
        }

        private void picksBansOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            // TODO
            return;
        }

        private partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
        {
            // As covers are displayed on stream, we want them to load as soon as possible.
            protected override double LoadDelay => 0;

            // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
            protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
                => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);
        }
    }
}
