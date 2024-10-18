// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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

        private const float initial_width = 0.98f;
        private const float shrink_width = 0.93f;

        private readonly string mod;
        private readonly string index;

        private Box colorBackground = null!;
        private SpriteIcon statusIcon = null!;
        private TournamentModIcon? modIcon;
        private Container beatmapInfoContainer = null!;
        private FillFlowContainer infoContainer = null!;
        private Box topMask = null!;
        private Box backgroundAddition = null!;
        private TournamentSpriteText instructText = null!;
        private Box slideBackground = null!;

        private FillFlowContainer charContainer = null!;

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
            for (int i = 0; i < songNameList.Length; i++)
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
                slideBackground = new Box
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    X = WIDTH,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                },
                statusIcon = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.Heart,
                    Size = new Vector2(24),
                    Colour = Color4.White,
                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                    Alpha = 0,
                },
                instructText = new TournamentSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.TorusAlternate.With(size: 36, weight: FontWeight.SemiBold),
                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                    Text = "This is a new map",
                    Alpha = 0,
                },
                charContainer = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                },
                beatmapInfoContainer = new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = initial_width,
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
                            Padding = new MarginPadding { Left = 15 },
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = truncatedSongName,
                                    Margin = new MarginPadding { Bottom = 5 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 32),
                                    Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(5),
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(6),
                                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                                            Children = new Drawable[]
                                            {
                                                Beatmap != null
                                                    ? new DifficultyIcon(Beatmap, ladder.Ruleset.Value)
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        TooltipType = DifficultyIconTooltipType.None,
                                                        Scale = new Vector2(0.9f),
                                                    }
                                                    : new Container(),
                                                new StarRatingDisplay(starDifficulty: new StarDifficulty(Beatmap?.StarRating ?? 0, 0), animated: true)
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                                new Container
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    AutoSizeAxes = Axes.Both,
                                                    Margin = new MarginPadding { Top = -3 }, // May not an elegant solution but it works
                                                    Child = new TournamentSpriteText
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Text = truncatedDifficultyName,
                                                        Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 23),
                                                    }
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
                            Margin = new MarginPadding { Bottom = -50, Right = 20 },
                            Size = new Vector2(96),
                            RelativeSizeAxes = Axes.Y,
                            Shear = new Vector2(-OsuGame.SHEAR, 0f),
                        },
                        topMask = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
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

        private BeatmapChoice? choice;

        private void updateState()
        {
            if (currentMatch.Value == null)
            {
                return;
            }

            var newChoice = currentMatch.Value.EXPicks.FirstOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            string choiceText = newChoice?.Team == TeamColour.Red ? "Red" :
                newChoice?.Team == TeamColour.Blue ? "Blue" : "Map";
            bool shouldFlash = newChoice != choice;

            if (newChoice != null)
            {
                beatmapInfoContainer.ResizeWidthTo(shrink_width, 200, Easing.OutQuint);

                if (shouldFlash)
                {
                    topMask.FadeTo(newChoice.Type == ChoiceType.Ban ? 0.5f : 0f, 300, Easing.OutQuint);

                    switch (newChoice.Type)
                    {
                        case ChoiceType.Pick:
                            statusIcon.Icon = FontAwesome.Solid.CheckCircle;
                            instructText.Text = "Map picked!";

                            backgroundAddition.FadeColour(Color4.White, 100, Easing.OutQuint);
                            backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.OutQuint);

                            runAnimation();
                            break;

                        case ChoiceType.Ban:
                            instructText.Text = $"{choiceText} banned!";
                            statusIcon.Icon = FontAwesome.Solid.Ban;

                            runAnimation(Color4.Gray);
                            break;

                        case ChoiceType.Protect:
                            instructText.Text = $"{choiceText} protected!";
                            statusIcon.Icon = FontAwesome.Solid.Lock;

                            runAnimation(newChoice.Team == TeamColour.Red ? new OsuColour().Red : new OsuColour().Sky);
                            break;

                        case ChoiceType.RedWin:
                            statusIcon.Icon = FontAwesome.Solid.Trophy;
                            instructText.Text = "Red wins!";

                            runAnimation(new OsuColour().Red);
                            break;

                        case ChoiceType.BlueWin:
                            statusIcon.Icon = FontAwesome.Solid.Trophy;
                            instructText.Text = "Blue wins!";

                            runAnimation(new OsuColour().Sky);
                            break;

                        default:
                            backgroundAddition.FadeColour(Color4.White, 100, Easing.OutQuint);
                            backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                            break;
                    }
                }
            }
            else
            {
                backgroundAddition.ClearTransforms();
                backgroundAddition.FadeOut(duration: 100, easing: Easing.OutCubic);
                backgroundAddition.Colour = Color4.White;
                beatmapInfoContainer.ResizeWidthTo(initial_width, 200, Easing.OutQuint);
                statusIcon.FadeOut(200, Easing.OutQuint);
                colorBackground.FadeColour(ColourInfo.GradientHorizontal(new Color4(1f, 1f, 1f, 0.4f), new Color4(0f, 0f, 0f, 0f)),
                    200, Easing.OutQuint);
                Alpha = 1;
            }

            choice = newChoice;
        }

        /// <summary>
        /// Start the animation sequence for the beatmap card.
        /// </summary>
        /// <param name="colour">The background colour in Step 2.</param>
        private void runAnimation(ColourInfo? colour = null)
        {
            // Stop any transform process (if exists) first
            ClearTransforms();

            ColourInfo useColour = colour ?? Color4.White;
            ColourInfo fadeColour = useColour == Color4.White ? Color4.Black : Color4.White;

            // Fade statusIcon out first, for positioning later
            statusIcon.FadeOut(150, Easing.OutQuint);

            using (BeginDelayedSequence(200))
            {
                slideBackground.X = WIDTH;
                slideBackground.Colour = Color4.White;
                slideBackground.Alpha = 1f;
                statusIcon.Colour = Color4.Black;
                instructText.Colour = Color4.Black;
                charContainer.Scale = new Vector2(1f, 1f);
                charContainer.X = WIDTH * 0.5f;
                beatmapInfoContainer.MoveToX(WIDTH, 200, Easing.OutQuint);

                using (BeginDelayedSequence(300))
                {
                    slideBackground.MoveToX(0, 500, Easing.OutQuint);
                }

                using (BeginDelayedSequence(700))
                {
                    statusIcon.X = WIDTH * 0.5f;
                    statusIcon.FadeIn(300, Easing.OutQuint);
                    statusIcon.ScaleTo(4.5f, 0) // Set initial scale to 4f instead
                              .Then().ScaleTo(2.2f, 400, Easing.OutQuint);
                }

                using (BeginDelayedSequence(1200))
                {
                    slideBackground.FadeColour(useColour, 1000, Easing.OutQuint);
                    statusIcon.FadeColour(fadeColour, 1000, Easing.OutQuint);
                    instructText.FadeColour(fadeColour, 1000, Easing.OutQuint);
                    statusIcon.MoveToX(WIDTH * 0.33f, 900, Easing.OutQuint);
                    charContainer.MoveToX(WIDTH * -0.05f, 900, Easing.OutQuint)
                                 .Then().Delay(200).ScaleTo(1.15f, 300, Easing.OutQuint);
                    instructText.MoveToX(-WIDTH * 0.55f, 900, Easing.OutExpo);

                    // Our function is not transformable, thus the sequence delay won't be applied.
                    fadeInByCharacter(instructText.Text.ToString(), 700, Easing.OutQuint, 1500, fadeColour);

                    // Execute backgroundAddition commands before the last delayed sequence
                    if (colour != null)
                    {
                        backgroundAddition.FadeColour(useColour, 1000, Easing.OutQuint);
                        backgroundAddition.FadeTo(0.5f, 1000, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(3000))
                    {
                        slideBackground.Delay(500).MoveToX(-WIDTH, 800, Easing.OutExpo);
                        slideBackground.FadeOut(1000, Easing.OutQuint);
                        statusIcon.MoveToX(WIDTH * 0.035f, 1000, Easing.OutExpo);
                        statusIcon.ScaleTo(1f, 1200, Easing.OutQuint);
                        beatmapInfoContainer.MoveToX(0, 1300, Easing.OutExpo);
                        // ↓ A swift movement would be too visual distracting.
                        instructText.MoveToX(0, 2000, Easing.OutQuint);
                        instructText.FadeOut(700, Easing.OutQuint);
                        beatmapInfoContainer.MoveToX(0, 1200, Easing.OutQuint);
                        colorBackground.FadeColour(ColourInfo.GradientHorizontal(useColour, Color4.White.Opacity(0)), 400, Easing.OutQuint);
                    }
                }
            }
        }

        /// <summary>
        /// Make each character in the given string to fade in one by one.
        /// </summary>
        /// <param name="text">The string to fade in.</param>
        /// <param name="duration">The duration for a character to fade in.</param>
        /// <param name="easing">The easing type for characters to use while fading in.</param>
        /// <param name="start">The time stamp when the fade in process starts.</param>
        /// <param name="step1Color">The initial colour when fading in.</param>
        private void fadeInByCharacter(string text, double duration, Easing easing, int start = 0,
                                       ColourInfo? step1Color = null)
        {
            // Ensure the character container is empty before adding new characters.
            charContainer.Clear();

            ColourInfo initialColor = step1Color ?? Color4.Black;

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];

                var charSpriteText = new TournamentSpriteText
                {
                    Text = character.ToString(),
                    Font = OsuFont.Torus.With(size: 32, weight: FontWeight.Bold),
                    Colour = initialColor,
                    Shadow = false,
                    Alpha = 0
                };

                charContainer.Add(charSpriteText);

                charSpriteText.Delay((int)(start + i * duration / text.Length))
                              .FadeIn(duration, easing);
            }
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
