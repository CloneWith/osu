// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;

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

        private WorkingBeatmap beatmap = null!;
        private ShowcasePlayer? player;
        private readonly List<ShowcaseBeatmap> beatmapSets;
        private readonly ShowcaseContainer showcaseContainer = null!;

        private readonly BindableBool replaying = new BindableBool();

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
                    player!.Delay(3000).Then().FadeOut(500, Easing.OutQuint);
                    Scheduler.AddDelayed(updateBeatmap, 3500);
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddInternal(new ShowcaseCountdownOverlay(config.StartCountdown.Value));

            Scheduler.AddDelayed(updateBeatmap, config.StartCountdown.Value);
        }

        private void updateBeatmap()
        {
            if (!beatmapSets.Any())
            {
                this.Exit();
                return;
            }

            var selected = beatmapSets.First();
            beatmapSets.Remove(beatmapSets.First());
            beatmap = beatmaps.GetWorkingBeatmap(new BeatmapInfo
            {
                ID = selected.BeatmapGuid,
                OnlineID = selected.BeatmapId
            }, true);
            var ruleset = rulesets.GetRuleset(beatmap.BeatmapInfo.Ruleset.OnlineID)?.CreateInstance();
            var autoplayMod = ruleset?.GetAutoplayMod();

            if (ruleset == null || autoplayMod == null)
                return;

            Beatmap.Value = beatmap;

            var score = autoplayMod.CreateScoreFromReplayData(beatmap.GetPlayableBeatmap(ruleset.RulesetInfo), Mods.Value);

            if (player != null)
                showcaseContainer.ScreenStack.Exit();

            showcaseContainer.ScreenStack.Push(player = new ShowcasePlayer(score, 0, config, replaying));
        }
    }
}
