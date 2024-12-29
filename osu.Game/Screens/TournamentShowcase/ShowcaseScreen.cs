// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseScreen : OsuScreen
    {
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

        private WorkingBeatmap beatmap = null!;
        private ShowcasePlayer? player;
        private readonly List<ShowcaseBeatmap> beatmapSets;
        private readonly ShowcaseContainer showcaseContainer = null!;

        private readonly BindableBool replaying = new BindableBool();
        private readonly Bindable<ShowcaseState> state = new Bindable<ShowcaseState>();

        public ShowcaseScreen(ShowcaseConfig config)
        {
            this.config = config;
            beatmapSets = config.Beatmaps.ToList();

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
                        showcaseContainer = new ShowcaseContainer(config, state, replaying)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            Width = relativeWidth,
                            Height = 0.95f * relativeHeight,
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

            showcaseContainer.BeatmapAttributes.Scale = new Vector2(priorityScale);
            showcaseContainer.BeatmapInfoDisplay.Scale = new Vector2(priorityScale);

            replaying.BindValueChanged(status =>
            {
                if (!status.NewValue && state.Value == ShowcaseState.BeatmapShow)
                {
                    showcaseContainer.BeatmapAttributes.FadeOut(500, Easing.OutQuint);
                    showcaseContainer.BeatmapInfoDisplay.FadeOut(500, Easing.OutQuint);
                    player!.Delay(3000).Then().FadeOut(500, Easing.OutQuint);
                    Scheduler.AddDelayed(pushNextBeatmap, 4500);
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Switch the ruleset beforehand to avoid cast exception.
            Ruleset.Value = config.FallbackRuleset.Value;

            AddInternal(new ShowcaseCountdownOverlay(config.StartCountdown.Value));
            state.BindValueChanged(stateChanged);
            Scheduler.AddDelayed(showcaseContainer.StartShowcase, config.StartCountdown.Value);
        }

        private void stateChanged(ValueChangedEvent<ShowcaseState> state)
        {
            switch (state.NewValue)
            {
                case ShowcaseState.Intro:
                    pushIntroBeatmap();
                    return;

                case ShowcaseState.BeatmapTransition:
                    pushNextBeatmap();
                    return;

                case ShowcaseState.Ended:
                    if (config.Layout.Value == ShowcaseLayout.Immersive)
                        Scheduler.AddDelayed(this.Exit, 5000);
                    return;

                default:
                    return;
            }
        }

        private void pushIntroBeatmap() => updateBeatmap(true);

        private void pushNextBeatmap() => updateBeatmap();

        /// <summary>
        /// Load the next beatmap in the queue and push it to the player.
        /// <br/>If no map presents, this will trigger the outro screen.
        /// </summary>
        private void updateBeatmap(bool introMode = false)
        {
            state.Value = introMode ? ShowcaseState.Intro : ShowcaseState.BeatmapShow;
            ShowcaseBeatmap selected;
            Score score;

            if (!introMode)
            {
                if (!beatmapSets.Any())
                {
                    state.Value = ShowcaseState.Ending;
                    return;
                }

                selected = beatmapSets.First();
                beatmapSets.Remove(beatmapSets.First());

                showcaseContainer.BeatmapInfoDisplay.MoveToX(-0.3f);
                showcaseContainer.BeatmapAttributes.FadeOut();
                showcaseContainer.BeatmapInfoDisplay.FadeOut();

                using (BeginDelayedSequence(1000))
                {
                    showcaseContainer.BeatmapAttributes.FadeIn(500, Easing.OutQuint);

                    showcaseContainer.BeatmapInfoDisplay.FadeIn(1000, Easing.OutQuint)
                                     .MoveToX(0.01f, 800, Easing.OutQuint);
                }
            }
            else
            {
                selected = config.UseCustomIntroBeatmap.Value ? config.IntroBeatmap.Value : config.Beatmaps.First();
                replaying.Value = false;
            }

            beatmap = beatmapManager.GetWorkingBeatmap(new BeatmapInfo
            {
                ID = selected.BeatmapGuid,
                OnlineID = selected.BeatmapId
            }, true);

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

                score = autoplayMod.CreateScoreFromReplayData(beatmap.GetPlayableBeatmap(ruleset.RulesetInfo), selected.RequiredMods);
            }

            Beatmap.Value = beatmap;
            showcaseContainer.BeatmapAttributes.BeatmapInfo.Value = beatmap.BeatmapInfo;
            showcaseContainer.BeatmapAttributes.Mods.Value = score.ScoreInfo.Mods.ToList();
            showcaseContainer.BeatmapInfoDisplay.Beatmap.Value = selected;

            Mods.Value = score.ScoreInfo.Mods;

            if (player != null)
                showcaseContainer.ScreenStack.Exit();

            showcaseContainer.ScreenStack.Push(player = new ShowcasePlayer(score, introMode ? beatmap.Metadata.PreviewTime : -1500, config, replaying, introMode));
        }
    }
}
