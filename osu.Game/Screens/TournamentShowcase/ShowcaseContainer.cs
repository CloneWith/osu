// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Localisation;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseContainer : Container
    {
        public OsuScreenStack ScreenStack { get; private set; }
        public ScreenStack ErrorStack { get; private set; }

        public readonly ShowcaseBeatmapInfoWedge InfoDisplay;

        private readonly PlayerContainer playerContainer;
        private readonly Box backgroundMask;
        private readonly Box topMask;
        private readonly TrianglesV2 triangles;
        private readonly WaveContainer transitionMask;
        private readonly Sprite transitionBackground;

        private readonly ShowcaseConfig config;
        private readonly float yPositionScale;
        private readonly float priorityScale;

        private readonly Bindable<ShowcaseState> state = new Bindable<ShowcaseState>();
        private readonly BindableBool playerLoaded = new BindableBool();

        public ShowcaseContainer(ShowcaseConfig config, Bindable<ShowcaseState> showcaseState, BindableBool playerLoaded)
        {
            this.config = config;
            state.BindTo(showcaseState);
            this.playerLoaded.BindTo(playerLoaded);

            yPositionScale = config.Layout.Value == ShowcaseLayout.Immersive ? 1 : 0.95f;
            priorityScale = Math.Min(config.AspectRatio.Value, 1f / config.AspectRatio.Value);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                backgroundMask = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0,
                },
                playerContainer = new PlayerContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = ScreenStack = new OsuScreenStack
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                triangles = new TrianglesV2
                {
                    RelativeSizeAxes = Axes.Both,
                    ScaleAdjust = 1.5f,
                    SpawnRatio = 1.75f,
                    Alpha = 0,
                },
                InfoDisplay = new ShowcaseBeatmapInfoWedge
                {
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.None,
                    Width = 400,
                    Alpha = 0,
                    X = -0.01f,
                    Y = 0.2f,
                },
                transitionMask = new WaveContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FirstWaveColour = FumoColours.SeaBlue.Lighter,
                    SecondWaveColour = FumoColours.SeaBlue.Light,
                    ThirdWaveColour = FumoColours.SeaBlue.Dark,
                    FourthWaveColour = FumoColours.SeaBlue.Darker,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FumoColours.SeaBlue.Regular,
                        },
                        transitionBackground = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                        }
                    }
                },
                topMask = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0,
                },
                ErrorStack = new ScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };

            state.BindValueChanged(stateChanged);
            this.playerLoaded.BindValueChanged(loadStateChanged);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            transitionBackground.Texture = textures.Get($"{config.TournamentName}/transition");
        }

        private void loadStateChanged(ValueChangedEvent<bool> state)
        {
            if (state.NewValue && this.state.Value == ShowcaseState.Intro)
                showIntro();
        }

        private void stateChanged(ValueChangedEvent<ShowcaseState> state)
        {
            switch (state.NewValue)
            {
                case ShowcaseState.BeatmapTransition:
                    showTransition();
                    break;

                case ShowcaseState.Ending:
                    showOutro();
                    return;

                default:
                    return;
            }
        }

        public void StartShowcase() => state.Value = ShowcaseState.Intro;

        /// <summary>
        /// Show the intro screen, fade the showcase container out and then exit.
        /// </summary>
        private void showIntro()
        {
            state.Value = ShowcaseState.Intro;
            backgroundMask.FadeIn(500, Easing.OutQuint);

            OsuLogo logo;
            OsuSpriteText titleText;
            OsuSpriteText subtitleText;

            Container introContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Alpha = 0,
                Children = new Drawable[]
                {
                    logo = new OsuLogo
                    {
                        RelativePositionAxes = Axes.Both,
                        X = -0.5f,
                        Y = 0.5f * yPositionScale,
                        Scale = new Vector2(0.5f * priorityScale),
                    },
                    titleText = new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 1.45f,
                        Y = 0.45f,
                        Alpha = 0,
                        Text = config.TournamentName.Value,
                        Font = OsuFont.GetFont(size: 80, typeface: Typeface.TorusAlternate, weight: FontWeight.SemiBold),
                        Scale = new Vector2(priorityScale)
                    },
                    subtitleText = new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 1.45f,
                        Y = 0.55f,
                        Alpha = 0,
                        Text = config.RoundName.Value,
                        Font = OsuFont.GetFont(size: 60, typeface: Typeface.TorusAlternate),
                        Scale = new Vector2(priorityScale)
                    },
                }
            };

            AddInternal(introContainer);

            using (BeginDelayedSequence(1500))
            {
                playerContainer.BlurTo(new Vector2(10), 1500, Easing.OutQuint);
                topMask.FadeTo(0.25f, 1500, Easing.OutQuint);
                triangles.FadeIn(1500, Easing.OutQuint);
                introContainer.FadeIn(1000, Easing.OutQuint);
            }

            using (BeginDelayedSequence(2700))
            {
                titleText.FadeIn(600, Easing.OutQuint);
                subtitleText.Delay(100).FadeIn(600, Easing.OutQuint);

                titleText.MoveToX(0.45f, 1000, Easing.OutQuint);
                subtitleText.Delay(100).MoveToX(0.45f, 1000, Easing.OutQuint);
            }

            logo.Delay(2700).FadeIn(500);
            logo.Delay(2700).MoveToX(0.25f, 1000, Easing.OutQuint);
            logo.Delay(2700).ScaleTo(new Vector2(0.8f * priorityScale), 500, Easing.OutQuint);

            using (BeginDelayedSequence(6000))
            {
                introContainer.FadeOut(1000, Easing.InQuint);
                introContainer.MoveToY(-1.5f, 1500, Easing.InQuint);
            }

            Scheduler.AddDelayed(showMapPool, 6000);
        }

        /// <summary>
        /// Show the map pool screen.
        /// </summary>
        private void showMapPool()
        {
            if (!config.ShowMapPool.Value)
            {
                state.Value = ShowcaseState.BeatmapTransition;
                return;
            }

            state.Value = ShowcaseState.MapPool;

            FillFlowContainer mapPoolFlow;

            Container mapPoolContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.Both,
                Y = 1f,
                Alpha = 0,
                Masking = true,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.Both,
                        Y = 0.1f,
                        Font = OsuFont.TorusAlternate.With(size: 30, weight: FontWeight.SemiBold),
                        Text = TournamentShowcaseStrings.MapPoolHeader,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.Both,
                        Y = 0.15f,
                        Font = OsuFont.TorusAlternate.With(size: 20),
                        Text = config.RoundName.Value,
                    },
                    mapPoolFlow = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        RelativePositionAxes = Axes.Both,
                        AutoSizeAxes = Axes.Y,
                        AutoSizeDuration = 300,
                        AutoSizeEasing = Easing.OutQuint,
                        Width = 0.9f,
                        Spacing = new Vector2(5),
                        Y = 0.2f,
                    }
                }
            };

            AddInternal(mapPoolContainer);

            using (BeginDelayedSequence(800))
            {
                mapPoolContainer.FadeIn(1000, Easing.OutQuint);
                mapPoolContainer.MoveToY(0, 1000, Easing.OutQuint);
            }

            var mapList = config.Beatmaps.ToList();
            // Adjust the width of each card based on real flow width
            float targetWidth = (mapPoolFlow.DrawWidth - 5 * 2) / 3;

            for (int i = 0; i * 3 < mapList.Count; i++)
            {
                var activeMaps = mapList.Skip(i * 3).Take(3).ToList();

                for (int j = 0; j < activeMaps.Count; j++)
                {
                    int j1 = j;
                    Scheduler.AddDelayed(() =>
                    {
                        var card = new ExtendableBeatmapCard(activeMaps[j1])
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = targetWidth,
                            Alpha = 0,
                        };

                        mapPoolFlow.Add(card);
                        card.MoveToY(card.Y + 100).Then().MoveToY(card.Y - 100, 500, Easing.OutQuint);
                        card.Delay(250).FadeIn(500, Easing.OutQuint);

                        Scheduler.AddDelayed(() => card.Shrink(), 2000 - j1 * 200);
                    }, i * 1000 + j * 200 + 800);
                }
            }

            int totalTime = mapList.Count * 1000 + 5000;

            using (BeginDelayedSequence(totalTime))
            {
                mapPoolContainer.MoveToY(-1f, 1500, Easing.InQuint);
                mapPoolContainer.FadeOut(1000, Easing.InQuint);

                topMask.Delay(1000).FadeOut(1000, Easing.InQuint);
                triangles.Delay(1000).FadeOut(1500, Easing.OutQuint);
                playerContainer.Delay(1000).BlurTo(Vector2.Zero, 1500, Easing.OutQuint);
            }

            Scheduler.AddDelayed(() =>
            {
                state.Value = ShowcaseState.BeatmapTransition;
            }, totalTime + 3000);
        }

        /// <summary>
        /// Show the transformation animation between replays.
        /// </summary>
        private void showTransition()
        {
            transitionMask.Show();
            Scheduler.AddDelayed(() =>
            {
                transitionMask.RotateTo(180);
                transitionBackground.RotateTo(180);
                transitionMask.Hide();
                transitionMask.Delay(600).RotateTo(0);
                transitionBackground.Delay(600).RotateTo(0);
            }, 600);
        }

        /// <summary>
        /// Show the outro screen, fade the showcase container out and then exit.
        /// </summary>
        private void showOutro()
        {
            state.Value = ShowcaseState.Ending;

            OsuLogo logo;

            Container outroContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.Both,
                Y = 1f,
                Masking = true,
                Alpha = 0,
                Children = new Drawable[]
                {
                    logo = new OsuLogo
                    {
                        RelativePositionAxes = Axes.Both,
                        X = 0.25f,
                        Y = 0.5f * yPositionScale,
                        Scale = new Vector2(0.5f * priorityScale),
                    },
                    new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 0.45f,
                        Y = 0.45f,
                        Text = !string.IsNullOrWhiteSpace(config.OutroTitle.Value?.Trim())
                            ? config.OutroTitle.Value.Trim()
                            : @"Thanks for watching!",
                        Font = OsuFont.GetFont(size: 80, typeface: Typeface.TorusAlternate, weight: FontWeight.SemiBold),
                        Scale = new Vector2(priorityScale)
                    },
                    new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 0.45f,
                        Y = 0.55f,
                        Text = !string.IsNullOrWhiteSpace(config.OutroSubtitle.Value?.Trim())
                            ? config.OutroSubtitle.Value.Trim()
                            : @"Take care of yourself, and be well.",
                        Font = OsuFont.GetFont(size: 60, typeface: Typeface.TorusAlternate),
                        Scale = new Vector2(priorityScale)
                    },
                }
            };

            AddInternal(outroContainer);

            using (BeginDelayedSequence(1500))
            {
                playerContainer.BlurTo(new Vector2(10), 1500, Easing.OutQuint);
                topMask.FadeTo(0.25f, 1500, Easing.OutQuint);
                triangles.FadeIn(1500, Easing.OutQuint);
                outroContainer.FadeIn(1500, Easing.OutQuint);
                outroContainer.MoveToY(0, 1000, Easing.OutQuint);
            }

            logo.Delay(1500).FadeIn(500);
            logo.Delay(1500).ScaleTo(new Vector2(0.8f * priorityScale), 500, Easing.OutQuint);

            using (BeginDelayedSequence(6000))
            {
                outroContainer.MoveToY(-1f, 1500, Easing.InQuint);
                outroContainer.FadeOut(1000, Easing.InQuint);
                triangles.FadeOut(1500, Easing.OutQuint);

                topMask.Delay(2500).FadeIn(1500, Easing.OutQuint);
            }

            Scheduler.AddDelayed(() => state.Value = ShowcaseState.Ended, 10000);
        }

        private partial class PlayerContainer : BufferedContainer
        {
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;
        }
    }
}
