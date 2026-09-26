// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Models;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseViewScreen : OsuScreen, ISubScreenWithTitle, IKeyBindingHandler<GlobalAction>
    {
        public string ShortTitle => @"Showcase";

        public bool ShowHeaderLine => false;

        private const float replay_preempt = -1500;
        private const float secret_map_preempt = replay_preempt - 3000;

        /// <summary>
        /// The delay between the beatmap change and the replay actually starting.
        /// </summary>
        private const float replay_start_delay = 500;

        /// <summary>
        /// How long the transient beatmap information display is kept before it starts fading out.
        /// <br/>This is paid for out of the extra lead-in granted to non-secret maps (see <see cref="secret_map_preempt"/>).
        /// </summary>
        private const float info_display_duration = 2500;

        /// <summary>
        /// When the persistent wedge takes over from the transient beatmap information display.
        /// </summary>
        private const float wedge_reveal_delay = replay_start_delay + info_display_duration + ShowcaseBeatmapIntroDisplay.CONCEAL_DURATION;

        /// <summary>
        /// How long to wait for the results screen of a secret map before revealing its information regardless.
        /// <br/>A replay which does not pass is never given a results screen at all, so this acts as a fallback.
        /// </summary>
        private const float secret_info_fallback_delay = 2000;

        /// <summary>
        /// The delay between a replay's result screen and the next replay.
        /// </summary>
        private const float result_duration = 8000;

        [Cached]
        private readonly ShowcaseConfig config;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? AllowGlobalTrackControl => false;

        public override bool AllowUserExit => false;

        public override bool HideOverlaysOnEnter => true;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        private int currentIndex;
        private WorkingBeatmap beatmap = null!;
        private ShowcasePlayer? player;
        private readonly List<ShowcaseBeatmap> beatmapSets;
        private readonly ShowcaseContainer showcaseContainer = null!;

        private readonly BindableBool replaying = new BindableBool();
        private readonly Bindable<ShowcaseState> state = new Bindable<ShowcaseState>();

        private ScheduledDelegate? scheduledNextPush;
        private ScheduledDelegate? scheduledErrorPush;
        private ScheduledDelegate? scheduledInfoDisplay;

        /// <summary>
        /// Whether the beatmap currently being showcased is a secret map, and its information should therefore
        /// only be revealed once its replay has ended.
        /// </summary>
        private bool currentMapIsSecret;

        public ShowcaseViewScreen(ShowcaseConfig config)
        {
            this.config = config;
            beatmapSets = config.Beatmaps.ToList();

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };

            float priorityScale = Math.Min(config.AspectRatio.Value, 1f / config.AspectRatio.Value);
            float relativeWidth = config.AspectRatio.Value < 1f ? config.AspectRatio.Value : 1;
            float relativeHeight = config.AspectRatio.Value < 1f ? 1 : 1f / config.AspectRatio.Value;

            switch (config.Layout.Value)
            {
                case ShowcaseLayout.Immersive:
                    InternalChild = showcaseContainer = new ShowcaseContainer(config, state, replaying)
                    {
                        Width = relativeWidth,
                        Height = relativeHeight,
                    };
                    break;

                case ShowcaseLayout.SimpleControl:
                    InternalChildren =
                    [
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                showcaseContainer = new ShowcaseContainer(config, state, replaying)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    Width = relativeWidth,
                                    Height = 0.95f * relativeHeight,
                                },
                                new SimpleShowcaseControl(this.Exit)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                            },
                        },
                    ];
                    break;

                case ShowcaseLayout.DetailedControl:
                    Padding = new MarginPadding
                    {
                        Horizontal = 20,
                        Vertical = 10,
                    };
                    InternalChildren =
                    [
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.95f,
                            RowDimensions =
                            [
                                new Dimension(),
                            ],
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    showcaseContainer = new ShowcaseContainer(config, state, replaying)
                                },
                            }
                        },
                        new HoldForMenuButton
                        {
                            Action = this.Exit,
                            Padding = new MarginPadding
                            {
                                Bottom = 90
                            },
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                        }
                    ];
                    break;
            }

            showcaseContainer.Wedge.Scale = new Vector2(priorityScale);

            replaying.BindValueChanged(status =>
            {
                if (!status.NewValue && state.Value == ShowcaseState.BeatmapShow)
                {
                    scheduledInfoDisplay?.Cancel();

                    showcaseContainer.Wedge.MoveToX(-0.75f, 800, Easing.InQuint);
                    showcaseContainer.Wedge.Delay(250).FadeOut(500, Easing.OutQuint);

                    player!.Delay(result_duration).Then().FadeOut(500, Easing.OutQuint);

                    scheduledNextPush = Scheduler.AddDelayed(() =>
                    {
                        if (showcaseContainer.UseAutoShowcase.Value)
                            pushNextBeatmap();
                    }, result_duration + 1000);

                    if (currentMapIsSecret)
                    {
                        // A secret map's information is only revealed once its replay has ended.
                        // The results screen is the intended moment, so this is only a fallback for when it never shows up.
                        scheduledInfoDisplay = Scheduler.AddDelayed(() => revealInfoDisplay(info_display_duration), secret_info_fallback_delay);
                    }
                }
            });

            showcaseContainer.OnPushNext += pushNextBeatmap;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Pause the music if playing.
            if (music.IsPlaying)
                music.TogglePause();

            // Switch the ruleset beforehand to avoid cast exception.
            Ruleset.Value = config.FallbackRuleset.Value;

            // The countdown overlay has an extra 1000ms delay, so we handle it here.
            AddInternal(new ShowcaseCountdownOverlay(config.StartCountdown.Value - 1000)
            {
                // Also add a small delay to make the transition between the countdown and intro smoother.
                ResumeAction = () => Scheduler.AddDelayed(showcaseContainer.StartShowcase, 500),
            });

            state.BindValueChanged(stateChanged);
        }

        private void handleFatalException(Exception e)
        {
            Logger.Error(e, "Exception caught in showcase. The showcase has been halted.");
            showcaseContainer.ErrorStack.Push(new ShowcaseErrorScreen(e, this.Exit));
        }

        private void scheduleBeatmapPush(Action pushAction, int delay = 0)
        {
            if (showcaseContainer.ErrorStack.CurrentScreen != null)
                showcaseContainer.ErrorStack.Exit();

            scheduledNextPush?.Cancel();
            scheduledNextPush = Scheduler.AddDelayed(pushAction, delay);
            scheduledErrorPush?.Cancel();

            hideInfoDisplay();
        }

        /// <summary>
        /// Show the transient beatmap information display, concealing it again after <paramref name="visibleDuration"/>.
        /// </summary>
        private void revealInfoDisplay(float visibleDuration)
        {
            scheduledInfoDisplay?.Cancel();

            showcaseContainer.IntroDisplay.Reveal();
            scheduledInfoDisplay = Scheduler.AddDelayed(() => showcaseContainer.IntroDisplay.Conceal(), visibleDuration);
        }

        /// <summary>
        /// Immediately get the transient beatmap information display out of the way.
        /// </summary>
        private void hideInfoDisplay()
        {
            scheduledInfoDisplay?.Cancel();
            showcaseContainer.IntroDisplay.Conceal(0);
        }

        private void stateChanged(ValueChangedEvent<ShowcaseState> state)
        {
            switch (state.NewValue)
            {
                case ShowcaseState.Intro:
                    pushIntroBeatmap();
                    return;

                // Note: Beatmap changing mechanism shouldn't be implemented here.
                case ShowcaseState.BeatmapTransition:
                    scheduledNextPush?.Cancel();
                    return;

                case ShowcaseState.Ended:
                    if (music.IsPlaying)
                        music.TogglePause();

                    if (config.Layout.Value == ShowcaseLayout.Immersive)
                        Scheduler.AddDelayed(this.Exit, 5000);
                    return;

                default:
                    return;
            }
        }

        private void pushIntroBeatmap() => updateBeatmap(0, true);

        private void pushPreviousBeatmap()
        {
            // Currently we only support travelling among beatmaps.
            if (currentIndex <= 0)
                return;

            try
            {
                updateBeatmap(currentIndex - 1);
            }
            catch (Exception e)
            {
                handleFatalException(e);
            }
        }

        private void pushNextBeatmap()
        {
            try
            {
                updateBeatmap(state.Value is ShowcaseState.Intro or ShowcaseState.MapPool ? 0 : currentIndex + 1);
            }
            catch (Exception e)
            {
                handleFatalException(e);
            }
        }

        /// <summary>
        /// Load the next beatmap in the queue and push it to the player.
        /// <br/>If no map presents, this will trigger the outro screen.
        /// </summary>
        private void updateBeatmap(int index, bool introMode = false)
        {
            ShowcaseBeatmap selected;
            Score score;

            if (!introMode)
            {
                if (index == beatmapSets.Count)
                {
                    state.Value = ShowcaseState.Ending;
                    return;
                }

                selected = beatmapSets[index];
                currentIndex = index;
                currentMapIsSecret = selected.IsSecretMap;

                // Get any leftover information display out of the way before this beatmap's own is shown.
                hideInfoDisplay();

                showcaseContainer.Wedge.MoveToX(-0.75f);
                showcaseContainer.Wedge.FadeOut();

                if (!currentMapIsSecret)
                {
                    // The persistent wedge only takes over once the transient information display has been concealed.
                    using (BeginDelayedSequence(wedge_reveal_delay))
                    {
                        showcaseContainer.Wedge.FadeIn(500, Easing.OutQuint)
                                         .MoveToX(-0.01f, 800, Easing.OutQuint);
                    }
                }

                state.Value = ShowcaseState.BeatmapTransition;
            }
            else
            {
                selected = config.UseCustomIntroBeatmap.Value ? config.IntroBeatmap.Value : config.Beatmaps.First();
                currentMapIsSecret = false;
                replaying.Value = false;

                hideInfoDisplay();
            }

            beatmap = beatmapManager.GetWorkingBeatmap(new BeatmapInfo
            {
                OnlineID = selected.BeatmapId,
                Hash = selected.BeatmapHash,
            }, true);

            if (beatmap.BeatmapInfo.BeatmapSet == null)
            {
                showcaseContainer.ErrorStack.Push(new ShowcaseBeatmapMissingScreen(selected));

                scheduledErrorPush = Scheduler.AddDelayed(() => scheduleBeatmapPush(pushNextBeatmap), 5000);
                return;
            }

            var ruleset = (rulesetStore.GetRuleset(selected.RulesetId) ?? config.FallbackRuleset.Value).CreateInstance();
            Ruleset.Value = ruleset.RulesetInfo;

            if (selected.ShowcaseScore != null)
            {
                var fetchedScore = scoreManager.GetScore(selected.ShowcaseScore);

                if (fetchedScore == null)
                {
                    Logger.Error(null, $"Could not find a score for {selected.ShowcaseScore}. Skipping.");
                    pushNextBeatmap();
                    return;
                }

                score = fetchedScore;
                Mods.Value = score.ScoreInfo.Mods;
            }
            else
            {
                var autoplayMod = ruleset.GetAutoplayMod();

                if (autoplayMod == null)
                {
                    Logger.Error(null, $"Unable to use the autoplay mod of {ruleset} for {selected.ShowcaseScore}. Skipping.");
                    pushNextBeatmap();
                    return;
                }

                score = autoplayMod.CreateScoreFromReplayData(beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, selected.RequiredMods), selected.RequiredMods);
                Mods.Value = selected.RequiredMods;
            }

            Scheduler.AddDelayed(() =>
            {
                Beatmap.Value = beatmap;
                showcaseContainer.Wedge.Target.Value = selected;
                state.Value = introMode ? ShowcaseState.Intro : ShowcaseState.BeatmapShow;

                if (player != null)
                    showcaseContainer.ScreenStack.Exit();

                player = new ShowcasePlayer(score, introMode ? beatmap.Metadata.PreviewTime : selected.IsSecretMap ? replay_preempt : secret_map_preempt,
                    config, selected, replaying, Mods.Value, introMode);

                player.OnError += handleFatalException;

                if (!introMode && selected.IsSecretMap)
                {
                    // A secret map's information is withheld until its replay is over, where the results screen reveals it.
                    player.OnShowingResults += () => revealInfoDisplay(info_display_duration);
                }

                showcaseContainer.ScreenStack.Push(player);

                if (!introMode && !selected.IsSecretMap)
                {
                    // The extra lead-in granted to non-secret maps (see secret_map_preempt) is spent on this display.
                    revealInfoDisplay(info_display_duration);
                }
            }, introMode ? 0 : replay_start_delay);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.ShowcaseToggleAuto:
                    if (!e.Repeat)
                        showcaseContainer.UseAutoShowcase.Value = !showcaseContainer.UseAutoShowcase.Value;

                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.ShowcaseForceQuit:
                    this.Exit();
                    break;

                case GlobalAction.ShowcasePrevious:
                    scheduleBeatmapPush(pushPreviousBeatmap);
                    break;

                case GlobalAction.ShowcaseNext:
                    scheduleBeatmapPush(pushNextBeatmap);
                    break;

                case GlobalAction.ShowcaseReplay:
                    if (state.Value is not ShowcaseState.BeatmapShow)
                        return;

                    scheduleBeatmapPush(() => updateBeatmap(currentIndex));
                    break;
            }
        }
    }
}
