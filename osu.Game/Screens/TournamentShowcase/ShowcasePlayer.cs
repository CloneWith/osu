// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcasePlayer : ReplayPlayer
    {
        private readonly Score score;
        private readonly ShowcaseConfig config;
        private readonly ShowcaseBeatmap beatmap;
        private readonly double startTime;
        private readonly bool noHUD;
        private readonly IReadOnlyList<Mod>? mods;

        private readonly float priorityScale;
        private readonly BindableBool replaying = new BindableBool();

        private TournamentOriginalBadge originalBadge = null!;

        public ShowcasePlayer(Score score, double startTime, ShowcaseConfig config, ShowcaseBeatmap beatmap, BindableBool replaying,
                              IReadOnlyList<Mod>? mods = null, bool noHUD = false)
            : base(score, new PlayerConfiguration
            {
                AllowUserInteraction = false,
                AllowFailAnimation = false
            })
        {
            this.score = score;
            this.startTime = startTime;
            this.mods = mods;
            this.beatmap = beatmap;
            this.replaying.BindTo(replaying);
            this.noHUD = noHUD;
            this.config = config;
            priorityScale = Math.Min(config.AspectRatio.Value, 1f / config.AspectRatio.Value);
        }

        protected override void LoadComplete()
        {
            Mods.Value = mods ?? score.ScoreInfo.Mods;
            base.LoadComplete();

            if (noHUD)
            {
                HUDOverlay.ShowHud.Value = false;
                HUDOverlay.ShowHud.Disabled = true;
                HUDOverlay.PlayfieldSkinLayer.Hide();
                FailOverlay.Hide();
                BreakOverlay.Hide();
                DrawableRuleset.Overlays.Hide();
                DrawableRuleset.Playfield.DisplayJudgements.Value = false;
            }

            // Adjust the scale and size of overlays.
            HUDOverlay.TopRightElements.Add(originalBadge = new TournamentOriginalBadge(@$"{config.TournamentName}/original-badge")
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.BottomRight,
                Scale = new Vector2(0.8f),
                Alpha = beatmap.IsOriginal.Value ? 1 : 0,
            });

            HUDOverlay.ScaleTo(new Vector2(priorityScale));
            HUDOverlay.ResizeTo(new Vector2(1f / priorityScale, 1f / priorityScale));

            BreakOverlay.ScaleTo(new Vector2(priorityScale));
            BreakOverlay.ResizeTo(new Vector2(1f / priorityScale, 1f / priorityScale));

            Scheduler.AddDelayed(() => originalBadge.Animate(), 800);

            Reset();
        }

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(score);
        }

        protected override void Update()
        {
            base.Update();

            if (GameplayState.HasPassed)
            {
                replaying.Value = false;
            }
        }

        protected override Score CreateScore(IBeatmap beatmap) => score;

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score)
        {
            Scale = new Vector2(priorityScale),
            Width = 1f / priorityScale,
            Height = 1f / priorityScale
        };

        public void Reset()
        {
            GameplayClockContainer.Stop();
            SetGameplayStartTime(startTime);
            GameplayClockContainer.Start();
            replaying.Value = true;
            this.FadeIn(200, Easing.In);
        }
    }
}
