// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class BoardBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        public readonly string Index;
        public readonly string Mod;

        public const float HEIGHT = 150;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private TournamentSpriteText instructText = null!;
        private Container infoContainer = null!;
        private FillFlowContainer textArea = null!;

        private Box slideBackground = null!;
        private Box backgroundAddition = null!;
        private Box flash = null!;

        private SpriteIcon icon = null!;
        private SpriteIcon swapIcon = null!;
        private SpriteIcon protectIcon = null!;
        private SpriteIcon trapIcon = null!;

        // Real X and Y positions on the board, distinct from RoundBeatmap.BoardX and BoardY.
        public int RealX;
        public int RealY;

        public BoardBeatmapPanel(IBeatmapInfo? beatmap, string mod = "", string index = "", int initX = 1, int initY = 1, float scale = 1.0f)
        {
            Beatmap = beatmap;
            Index = index;
            Mod = mod;
            RealX = initX;
            RealY = initY;

            Width = HEIGHT * scale;
            Height = HEIGHT * scale;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            var displayTitle = Beatmap?.GetDisplayTitleRomanisable(false, false)
                                      .ToString().ExtractSongTitleFromMetadata().Trim().TruncateWithEllipsis(17) ?? (LocalisableString)@"unknown";

            string displayDifficulty = Beatmap?.DifficultyName.TruncateWithEllipsis(19) ?? "unknown";

            Masking = true;

            InternalChildren = new Drawable[]
            {
                slideBackground = new Box
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -Height,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                },
                instructText = new TournamentSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = Height,
                    Font = OsuFont.TorusAlternate.With(size: 20, weight: FontWeight.SemiBold),
                    Text = "This is a new map",
                    Alpha = 0,
                },
                infoContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.Gray(0.5f),
                            OnlineInfo = (Beatmap as IBeatmapSetOnlineInfo),
                        },
                        backgroundAddition = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.1f
                        },
                        textArea = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Padding = new MarginPadding(15),
                            Direction = FillDirection.Vertical,

                            /* This section of code adds Beatmap Information to the Board grid. */
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = displayTitle,
                                    Padding = new MarginPadding { Left = 0 },
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 18),
                                    Margin = new MarginPadding { Left = -9, Top = -7 },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Margin = new MarginPadding { Left = -9, Top = 5 }, // Adjust this value to change the distance
                                    Child = new TournamentSpriteText
                                    {
                                        Text = displayDifficulty,
                                        MaxWidth = 120,
                                        Font = OsuFont.Torus.With(weight: FontWeight.Medium, size: 14),
                                    },
                                }
                            },
                        },
                        swapIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Yellow,
                            Size = new Vector2(0.4f),
                            Icon = FontAwesome.Solid.ExchangeAlt,
                            Alpha = 0,
                        },
                        protectIcon = new SpriteIcon
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Icon = FontAwesome.Solid.Lock,
                            Colour = Color4.White,
                            Size = new Vector2(24),
                            Position = new Vector2(5, -5),
                            Alpha = 0,
                        },
                        trapIcon = new SpriteIcon
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Icon = FontAwesome.Solid.ExclamationCircle,
                            Colour = Color4.White,
                            Size = new Vector2(24),
                            Position = new Vector2(5, -5),
                            Alpha = 0,
                        },
                        new TournamentModIcon(Index.IsNull() ? Mod : Mod + Index)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.Centre,
                            Margin = new MarginPadding(10),
                            Width = 60,
                            Size = new Vector2(80),
                            RelativeSizeAxes = Axes.Y,
                            Position = new Vector2(34, -18) // (x, y). Increment of x = Move right; Decrement of y = Move upwards.
                        }
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new Vector2(0.4f),
                    Alpha = 0,
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            };
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
            {
                match.OldValue.PicksBans.CollectionChanged -= picksBansOnCollectionChanged;
                match.OldValue.Protects.CollectionChanged -= picksBansOnCollectionChanged;
                match.OldValue.Traps.CollectionChanged -= picksBansOnCollectionChanged;
                match.OldValue.PendingSwaps.CollectionChanged -= picksBansOnCollectionChanged;
            }

            if (match.NewValue != null)
            {
                match.NewValue.PicksBans.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.Protects.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.Traps.CollectionChanged += picksBansOnCollectionChanged;
                match.NewValue.PendingSwaps.CollectionChanged += picksBansOnCollectionChanged;
            }

            Scheduler.AddOnce(updateState);
        }

        private void picksBansOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState);

        private BeatmapChoice? bpChoice, pChoice;
        private TrapInfo? tChoice;

        private void updateState()
        {
            if (currentMatch.Value == null)
            {
                return;
            }

            bool isBothTrapped = currentMatch.Value.Traps.Any(p => p.BeatmapID == Beatmap?.OnlineID && p.Team == TeamColour.Red)
                                 && currentMatch.Value.Traps.Any(p => p.BeatmapID == Beatmap?.OnlineID && p.Team == TeamColour.Blue);

            var newBpChoice = currentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var protectChoice = currentMatch.Value.Protects.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var trapChoice = currentMatch.Value.Traps.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var swapChoice = currentMatch.Value.PendingSwaps.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            var pickerChoice = currentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == Beatmap?.OnlineID && p.Type == ChoiceType.Pick);

            TeamColour protectColor = protectChoice?.Team ?? TeamColour.Neutral;
            TeamColour trapColor = trapChoice?.Team ?? TeamColour.Neutral;

            // Flash when new changes are made.
            bool shouldFlash = newBpChoice != bpChoice || protectChoice != pChoice || trapChoice != tChoice || swapChoice != null;

            string choiceText = newBpChoice?.Team == TeamColour.Red ? "Red" :
                newBpChoice?.Team == TeamColour.Blue ? "Blue" : "Map";

            if (newBpChoice != null || protectChoice != null || trapChoice != null || swapChoice != null)
            {
                if (shouldFlash)
                {
                    flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, 3);
                }

                if (protectChoice != null && trapChoice != null)
                {
                    trapIcon.MoveTo(newPosition: new Vector2(30, -5), duration: 200, easing: Easing.OutCubic);
                }
                else
                {
                    trapIcon.MoveTo(newPosition: new Vector2(5, -5), duration: 150, easing: Easing.OutCubic);
                }

                protectIcon.FadeTo(newAlpha: protectChoice != null ? 1 : 0, duration: 200, easing: Easing.OutCubic);
                trapIcon.FadeTo(newAlpha: trapChoice != null ? 1 : 0, duration: 200, easing: Easing.OutCubic);

                BorderThickness = 4;

                if (pickerChoice != null)
                {
                    BorderColour = TournamentGame.GetTeamColour(pickerChoice.Team);
                }
                else
                {
                    BorderColour = Color4.White;
                }

                if (swapChoice != null)
                {
                    swapIcon.RotateTo(0);
                    swapIcon.FadeInFromZero(duration: 100, easing: Easing.InCubic);
                }

                if (trapChoice != null)
                {
                    BorderThickness = 0;
                    Alpha = 1f;
                    icon.Icon = FontAwesome.Solid.ExclamationCircle;
                    instructText.Text = $"{choiceText} set a trap!";
                    if (trapChoice != tChoice) runAnimation(isBothTrapped ? Color4.White : trapColor == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky, false);
                    backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);

                    using (BeginDelayedSequence(3000))
                    {
                        trapIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                        trapIcon.Colour = isBothTrapped ? Color4.White : trapColor == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                    }
                }

                if (protectChoice != null && trapChoice == null)
                {
                    BorderThickness = 0;
                    Alpha = 1f;
                    icon.Icon = FontAwesome.Solid.Lock;
                    instructText.Text = $"{choiceText} protected!";
                    if (protectChoice != pChoice) runAnimation(TournamentGame.GetTeamColour(protectChoice.Team), false);
                    backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);

                    protectIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                    protectIcon.Colour = protectColor == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                }

                if (newBpChoice != null)
                {
                    textArea.FadeTo(newAlpha: newBpChoice.Type == ChoiceType.Ban ? 0.5f : 1f, duration: 200, easing: Easing.OutCubic);

                    switch (newBpChoice.Type)
                    {
                        case ChoiceType.Pick:
                            Colour = Color4.White;
                            Alpha = 1f;
                            backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Check;
                            BorderColour = TournamentGame.GetTeamColour(newBpChoice.Team);
                            BorderThickness = 4;
                            instructText.Text = $"{choiceText} picked!";
                            if (shouldFlash) runAnimation(TournamentGame.GetTeamColour(newBpChoice.Team), false);
                            break;

                        // Ban: All darker
                        case ChoiceType.Ban:
                            backgroundAddition.Colour = Color4.Black;
                            backgroundAddition.FadeTo(newAlpha: 0.7f, duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Ban;
                            BorderThickness = 0;
                            instructText.Text = $"{choiceText} banned!";
                            if (shouldFlash) runAnimation(newBpChoice.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky);
                            break;

                        // Win: Background colour
                        case ChoiceType.RedWin:
                            backgroundAddition.Colour = Color4.Red;
                            backgroundAddition.FadeTo(newAlpha: 0.4f, duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Trophy;
                            BorderThickness = 4;
                            instructText.Text = "Red wins!";
                            if (shouldFlash) runAnimation(new OsuColour().Red);
                            break;

                        case ChoiceType.BlueWin:
                            backgroundAddition.Colour = new OsuColour().Sky;
                            backgroundAddition.FadeTo(newAlpha: 0.5f, duration: 150, easing: Easing.InCubic);
                            icon.Icon = FontAwesome.Solid.Trophy;
                            BorderThickness = 4;
                            instructText.Text = "Blue wins!";
                            if (shouldFlash) runAnimation(new OsuColour().Blue);
                            break;

                        default:
                            icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                            BorderThickness = 0;
                            break;
                    }
                }
            }
            else
            {
                // Stop all transforms first, to make relative properties adjustable.
                icon.ClearTransforms();
                protectIcon.ClearTransforms();
                trapIcon.ClearTransforms();
                flash.ClearTransforms();
                textArea.ClearTransforms();

                // Then we can change them to the default state.
                BorderThickness = 0;
                flash.FadeOut(duration: 200, easing: Easing.OutCubic);
                this.FadeIn(duration: 100, easing: Easing.InCubic);
                backgroundAddition.FadeOut(duration: 100, easing: Easing.OutCubic);
                icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                protectIcon.FadeOut(duration: 100, easing: Easing.OutCubic);
                trapIcon.FadeOut(duration: 100, easing: Easing.OutCubic);
                textArea.FadeTo(newAlpha: 1f, duration: 200, easing: Easing.OutCubic);
                Colour = Color4.White;
                icon.Colour = Color4.White;
                backgroundAddition.Colour = Color4.White;
            }

            bpChoice = newBpChoice;
            pChoice = protectChoice;
            tChoice = trapChoice;
        }

        public void Flash(int count = 1)
        {
            if (count <= 0) return;

            if (count == 1) flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine);
            else flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, count);

            swapIcon.FadeOutFromOne(1000, Easing.InCubic);
        }

        /// <summary>
        /// Start the animation sequence for the board block.
        /// </summary>
        /// <param name="colour">The background colour in Step 2.</param>
        /// <param name="showIconAfterComplete">Whether to show the icon at the centre after the animation finishes.</param>
        private void runAnimation(ColourInfo? colour = null, bool showIconAfterComplete = true)
        {
            // Stop any transform process (if exists) first
            ClearTransforms();

            ColourInfo useColour = colour ?? Color4.White;
            ColourInfo fadeColour = useColour == Color4.White ? Color4.Black : Color4.White;

            // Fade statusIcon out first, for positioning later
            icon.FadeOut(150, Easing.OutQuint);

            using (BeginDelayedSequence(200))
            {
                slideBackground.Y = -Height;
                slideBackground.Colour = Color4.White;
                slideBackground.Alpha = 1f;
                icon.Colour = Color4.Black;
                instructText.Y = Height;
                instructText.Colour = Color4.Black;
                infoContainer.MoveToY(Height, 200, Easing.OutQuint);

                using (BeginDelayedSequence(300))
                {
                    slideBackground.MoveToY(0, 500, Easing.OutQuint);
                }

                using (BeginDelayedSequence(700))
                {
                    icon.FadeIn(300, Easing.OutQuint);
                    icon.ScaleTo(3f, 0)
                        .Then().ScaleTo(1.2f, 400, Easing.OutQuint);
                }

                using (BeginDelayedSequence(1200))
                {
                    slideBackground.FadeColour(useColour, 1000, Easing.OutQuint);
                    icon.FadeColour(fadeColour, 1000, Easing.OutQuint);
                    instructText.FadeIn(800, Easing.OutQuint);
                    instructText.FadeColour(fadeColour, 1000, Easing.OutQuint);
                    instructText.MoveToY(-Height * 0.1f, 900, Easing.OutExpo);

                    // Execute backgroundAddition commands before the last delayed sequence
                    if (colour != null)
                    {
                        // backgroundAddition.FadeColour(useColour, 1000, Easing.OutQuint);
                        // backgroundAddition.FadeTo(0.5f, 1000, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(3000))
                    {
                        slideBackground.Delay(500).MoveToY(-Height, 800, Easing.OutExpo);
                        slideBackground.FadeOut(1000, Easing.OutQuint);
                        icon.ScaleTo(1f, 1200, Easing.OutQuint);
                        icon.FadeColour(useColour, 1200, Easing.OutQuint);

                        // Hide icons under special circumstances (e.g. picks).
                        if (!showIconAfterComplete)
                        {
                            icon.Delay(500).FadeOut(1000, Easing.OutQuint);
                        }

                        // A swift movement would be too visual distracting.
                        instructText.MoveToY(0, 2000, Easing.OutQuint);
                        instructText.FadeOut(700, Easing.OutQuint);
                        infoContainer.MoveToY(0, 1200, Easing.OutExpo);
                    }
                }
            }
        }

        public void RotateSwapIconTo(float angle = 0, int delay = 0)
        {
            swapIcon.Delay(delay).RotateTo(angle, 800, Easing.OutQuint);
        }
    }
}
