// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

            using (BeginDelayedSequence(6000))
            {
                introContainer.FadeOut(1000, Easing.OutQuint);
                Scheduler.AddDelayed(showTeamPlayer, 3000);
            }
        }

        /// <summary>
        /// Show the round team list.
        /// </summary>
        private void showTeamPlayer()
        {
            state.Value = ShowcaseState.TeamPlayer;
            Scheduler.AddDelayed(showMapPool, 5000);
        }

        /// <summary>
        /// Show the map pool screen.
        /// </summary>
        private void showMapPool()
        {
            state.Value = ShowcaseState.MapPool;
            Scheduler.AddDelayed(_ =>
            {
                state.Value = ShowcaseState.BeatmapTransition;
            }, false, 5000);
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
                        Text = !string.IsNullOrWhiteSpace(config.OutroTitle.Value.Trim())
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
                        Text = !string.IsNullOrWhiteSpace(config.OutroSubtitle.Value.Trim())
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
