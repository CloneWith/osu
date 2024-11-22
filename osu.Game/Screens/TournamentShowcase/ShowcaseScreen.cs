// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
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
                    player!.Delay(3000).Then().FadeOut(500, Easing.OutQuint);
                    Scheduler.AddDelayed(updateBeatmap, 3500);
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Switch the ruleset beforehand to avoid cast exception.
            Ruleset.Value = config.Ruleset.Value;

            AddInternal(new ShowcaseCountdownOverlay(config.StartCountdown.Value));
            Scheduler.AddDelayed(updateBeatmap, config.StartCountdown.Value);
        }

        /// <summary>
        /// Load the next beatmap in the queue and push it to the player.
        /// If no map presents, this will trigger the outro screen.
        /// </summary>
        private void updateBeatmap()
        {
            if (!beatmapSets.Any())
            {
                showOutro();
                return;
            }

            var selected = beatmapSets.First();
            beatmapSets.Remove(beatmapSets.First());
            beatmap = beatmaps.GetWorkingBeatmap(new BeatmapInfo
            {
                ID = selected.BeatmapGuid,
                OnlineID = selected.BeatmapId
            }, true);

            var ruleset = config.Ruleset.Value?.CreateInstance();
            var autoplayMod = ruleset?.GetAutoplayMod();

            if (ruleset == null || autoplayMod == null)
                return;

            Beatmap.Value = beatmap;

            var score = autoplayMod.CreateScoreFromReplayData(beatmap.GetPlayableBeatmap(ruleset.RulesetInfo), Mods.Value);

            if (player != null)
                showcaseContainer.ScreenStack.Exit();

            showcaseContainer.ScreenStack.Push(player = new ShowcasePlayer(score, 0, config, replaying));
        }

        /// <summary>
        /// Show the outro screen, fade the showcase container out and then exit.
        /// </summary>
        private void showOutro()
        {
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
