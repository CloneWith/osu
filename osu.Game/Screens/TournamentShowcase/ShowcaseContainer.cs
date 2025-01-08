// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseContainer : CompositeDrawable
    {
        [Resolved]
        private OsuLogo? logo { get; set; }

        public OsuScreenStack ScreenStack { get; private set; }

        public readonly BeatmapAttributesDisplay BeatmapAttributes;
        public readonly ShowcaseBeatmapInfoArea BeatmapInfoDisplay;

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
                new PlayerContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = ScreenStack = new OsuScreenStack
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                BeatmapAttributes = new BeatmapAttributesDisplay
                {
                    RelativePositionAxes = Axes.Both,
                    Alpha = 0,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -0.01f,
                    Y = -0.1f,
                    Mods = { Value = new List<Mod>() },
                    Collapsed = { Value = false }
                },
                BeatmapInfoDisplay = new ShowcaseBeatmapInfoArea
                {
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.16f,
                    Alpha = 0,
                    X = 0.01f,
                    Y = 0.2f
                }
            };

            state.BindValueChanged(stateChanged);
            this.playerLoaded.BindValueChanged(loadStateChanged);
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

            Container introContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Alpha = 0,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 0.45f,
                        Y = 0.45f,
                        Text = config.TournamentName.Value,
                        Font = OsuFont.GetFont(size: 80, typeface: Typeface.TorusAlternate, weight: FontWeight.SemiBold),
                        Scale = new Vector2(priorityScale)
                    },
                    new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 0.45f,
                        Y = 0.55f,
                        Text = config.RoundName.Value,
                        Font = OsuFont.GetFont(size: 60, typeface: Typeface.TorusAlternate),
                        Scale = new Vector2(priorityScale)
                    },
                }
            };

            AddInternal(introContainer);

            introContainer.Delay(3000).FadeIn(1000, Easing.OutQuint);

            // Initialization
            logo?.Show();
            logo?.MoveTo(new Vector2(-0.5f, 0.5f * yPositionScale));
            logo?.ScaleTo(0.5f * priorityScale);

            logo?.Delay(3000).FadeIn(500);
            logo?.Delay(3000).MoveTo(new Vector2(0.25f, 0.5f * yPositionScale), 1000, Easing.OutQuint);
            logo?.Delay(4200).ScaleTo(new Vector2(0.8f * priorityScale), 500, Easing.OutQuint);

            logo?.Delay(6000).FadeOut(3000, Easing.OutQuint);

            introContainer.Delay(6000).FadeOut(1000, Easing.OutQuint);
            Scheduler.AddDelayed(showTeamPlayer, 3000 + 6000);
        }

        /// <summary>
        /// Show the round team list.
        /// </summary>
        private void showTeamPlayer()
        {
            state.Value = ShowcaseState.TeamPlayer;
            Scheduler.AddDelayed(showMapPool, config.ShowTeamList.Value ? 5000 : 0);
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

            OsuSpriteText mapPoolHeaderText, mapPoolSubText;
            FillFlowContainer mapPoolFlow;

            Container mapPoolContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    mapPoolHeaderText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.Both,
                        Y = -0.3f,
                        Font = OsuFont.TorusAlternate.With(size: 30, weight: FontWeight.SemiBold),
                        Text = @"Map Pool",
                    },
                    mapPoolSubText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.Both,
                        Y = -0.3f,
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
            mapPoolContainer.FadeIn(1000, Easing.OutQuint);

            using (BeginDelayedSequence(800))
            {
                mapPoolHeaderText.MoveToY(0.1f, 500, Easing.OutQuint);
                mapPoolSubText.Delay(100).MoveToY(0.15f, 500, Easing.OutQuint);
            }

            var mapList = config.Beatmaps.ToList();

            for (int i = 0; i * 3 < mapList.Count; i++)
            {
                var activeMaps = mapList.Skip(i * 3).Take(3).ToList();

                for (int j = 0; j < activeMaps.Count; j++)
                {
                    int j1 = j;
                    Scheduler.AddDelayed(_ =>
                    {
                        var card = new ExtendableBeatmapCard(activeMaps[j1], config)
                        {
                            Alpha = 0
                        };

                        mapPoolFlow.Add(card);
                        card.MoveToY(card.Y + 100).Then().MoveToY(card.Y - 100, 500, Easing.OutQuint);
                        card.Delay(100).FadeIn(500, Easing.OutQuint);

                        using (BeginDelayedSequence(2000 - j1 * 200))
                        {
                            card.Shrink();
                        }
                    }, false, i * 1000 + j * 200 + 800);
                }
            }

            int totalTime = mapList.Count * 1000 + 5000;

            using (BeginDelayedSequence(totalTime))
            {
                mapPoolContainer.FadeOut(1000, Easing.OutQuint);
            }

            Scheduler.AddDelayed(_ =>
            {
                state.Value = ShowcaseState.BeatmapTransition;
            }, false, totalTime + 2000);
        }

        /// <summary>
        /// Show the outro screen, fade the showcase container out and then exit.
        /// </summary>
        private void showOutro()
        {
            state.Value = ShowcaseState.Ending;

            BeatmapInfoDisplay.FadeOut(500, Easing.OutQuint);
            BeatmapAttributes.FadeOut(500, Easing.OutQuint);

            Container outroContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Alpha = 0,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
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

            // Initialization
            logo?.Show();
            logo?.MoveTo(new Vector2(-0.5f, 0.5f * yPositionScale));
            logo?.ScaleTo(0.5f * priorityScale);

            logo?.FadeIn(500);
            logo?.MoveTo(new Vector2(0.25f, 0.5f * yPositionScale), 1000, Easing.OutQuint);
            logo?.Delay(200).ScaleTo(new Vector2(0.8f * priorityScale), 500, Easing.OutQuint);

            outroContainer.FadeIn(1000, Easing.OutQuint);
            logo?.Delay(3000).FadeOut(3000, Easing.OutQuint);

            using (BeginDelayedSequence(3000))
            {
                this.FadeOut(3000, Easing.OutQuint);
                state.Value = ShowcaseState.Ended;
            }
        }

        private partial class PlayerContainer : Container
        {
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;
        }
    }
}
