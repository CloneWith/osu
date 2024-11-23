// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseScreen : OsuScreen
    {
        private readonly ShowcaseConfig config;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? AllowGlobalTrackControl => false;

        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private OsuLogo? logo { get; set; }

        private WorkingBeatmap beatmap = null!;
        private ShowcasePlayer? player;
        private readonly List<ShowcaseBeatmap> beatmapSets;
        private readonly ShowcaseContainer showcaseContainer = null!;

        private readonly BindableBool replaying = new BindableBool();
        private ShowcaseState state;

        public ShowcaseScreen(ShowcaseConfig config)
        {
            this.config = config;
            beatmapSets = config.Beatmaps.ToList();

            switch (config.Layout.Value)
            {
                case ShowcaseLayout.Immersive:
                    InternalChild = showcaseContainer = new ShowcaseContainer();
                    break;

                case ShowcaseLayout.SimpleControl:
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
                                    showcaseContainer = new ShowcaseContainer()
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
                                    showcaseContainer = new ShowcaseContainer()
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

            replaying.BindValueChanged(status =>
            {
                if (!status.NewValue)
                {
                    showcaseContainer.BeatmapAttributes.FadeOut(500, Easing.OutQuint);
                    player!.Delay(3000).Then().FadeOut(500, Easing.OutQuint);
                    Scheduler.AddDelayed(pushNextBeatmap, 3500);
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Switch the ruleset beforehand to avoid cast exception.
            Ruleset.Value = config.Ruleset.Value;

            AddInternal(new ShowcaseCountdownOverlay(config.StartCountdown.Value));
            Scheduler.AddDelayed(showIntro, config.StartCountdown.Value);
        }

        private void pushIntroBeatmap() => updateBeatmap(true);

        private void pushNextBeatmap() => updateBeatmap();

        /// <summary>
        /// Load the next beatmap in the queue and push it to the player.
        /// If no map presents, this will trigger the outro screen.
        /// </summary>
        private void updateBeatmap(bool introMode = false)
        {
            state = ShowcaseState.BeatmapShow;
            ShowcaseBeatmap selected;
            Score? score;

            if (!introMode)
            {
                if (!beatmapSets.Any())
                {
                    showOutro();
                    return;
                }

                selected = beatmapSets.First();
                beatmapSets.Remove(beatmapSets.First());
                showcaseContainer.BeatmapAttributes.Delay(1000).FadeIn(500, Easing.OutQuint);
            }
            else
            {
                selected = config.IntroBeatmap.Value;
            }

            beatmap = beatmaps.GetWorkingBeatmap(new BeatmapInfo
            {
                ID = selected.BeatmapGuid,
                OnlineID = selected.BeatmapId
            }, true);

            var ruleset = config.Ruleset.Value?.CreateInstance();

            if (selected.ShowcaseScore != null)
            {
                score = scoreManager.GetScore(selected.ShowcaseScore);
            }
            else
            {
                var autoplayMod = ruleset?.GetAutoplayMod();
                if (ruleset == null || autoplayMod == null)
                    return;

                score = autoplayMod.CreateScoreFromReplayData(beatmap.GetPlayableBeatmap(ruleset.RulesetInfo), Mods.Value);
            }

            Beatmap.Value = beatmap;
            showcaseContainer.BeatmapAttributes.BeatmapInfo.Value = beatmap.BeatmapInfo;
            showcaseContainer.BeatmapAttributes.Mods.Value = score?.ScoreInfo.Mods.ToList() ?? selected.RequiredMods.ToList();

            if (score == null)
            {
                Logger.Error(null, $"Could not find a score for {selected.ShowcaseScore}. Skipping.");
                pushNextBeatmap();
                return;
            }

            Mods.Value = score.ScoreInfo.Mods;

            if (player != null)
                showcaseContainer.ScreenStack.Exit();

            showcaseContainer.ScreenStack.Push(player = new ShowcasePlayer(score, introMode ? beatmap.Metadata.PreviewTime : -1500, config, replaying, introMode));
        }

        /// <summary>
        /// Show the intro screen, fade the showcase container out and then exit.
        /// </summary>
        private void showIntro()
        {
            state = ShowcaseState.Intro;

            Container introContainer = new Container
            {
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
                    },
                    new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.CentreLeft,
                        X = 0.45f,
                        Y = 0.55f,
                        Text = config.RoundName.Value,
                        Font = OsuFont.GetFont(size: 60, typeface: Typeface.TorusAlternate),
                    },
                }
            };

            AddInternal(introContainer);
            pushIntroBeatmap();

            introContainer.Delay(3000).FadeIn(1000, Easing.OutQuint);

            // Initialization
            logo?.Show();
            logo?.MoveTo(new Vector2(-0.5f, 0.5f));
            logo?.ScaleTo(0.5f);

            logo?.Delay(3000).FadeIn(500);
            logo?.Delay(3000).MoveTo(new Vector2(0.25f, 0.5f), 1000, Easing.OutQuint);
            logo?.Delay(4200).ScaleTo(new Vector2(0.8f), 500, Easing.OutQuint);

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
            state = ShowcaseState.TeamPlayer;
            Scheduler.AddDelayed(showMapPool, 5000);
        }

        /// <summary>
        /// Show the map pool screen.
        /// </summary>
        private void showMapPool()
        {
            state = ShowcaseState.MapPool;
            Scheduler.AddDelayed(pushNextBeatmap, 5000);
        }

        /// <summary>
        /// Show the outro screen, fade the showcase container out and then exit.
        /// </summary>
        private void showOutro()
        {
            state = ShowcaseState.Ending;

            Container outroContainer = new Container
            {
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
                    },
                }
            };

            AddInternal(outroContainer);

            // Initialization
            logo?.Show();
            logo?.MoveTo(new Vector2(-0.5f, 0.5f));
            logo?.ScaleTo(0.5f);

            logo?.FadeIn(500);
            logo?.MoveTo(new Vector2(0.25f, 0.5f), 1000, Easing.OutQuint);
            logo?.Delay(200).ScaleTo(new Vector2(0.8f), 500, Easing.OutQuint);

            outroContainer.FadeIn(1000, Easing.OutQuint);
            logo?.Delay(3000).FadeOut(3000, Easing.OutQuint);

            using (BeginDelayedSequence(3000))
            {
                this.FadeOut(3000, Easing.OutQuint);
                Scheduler.AddDelayed(this.Exit, 5000);
            }
        }
    }
}
