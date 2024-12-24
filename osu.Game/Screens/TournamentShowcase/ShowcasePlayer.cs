// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcasePlayer : ReplayPlayer
    {
        private readonly ShowcaseConfig config;
        private readonly Score score;
        private readonly double startTime;
        private readonly bool noHUD;

        private readonly float priorityScale;
        private readonly BindableBool replaying = new BindableBool();

        public ShowcasePlayer(Score score, double startTime, ShowcaseConfig config, BindableBool replaying, bool noHUD = false)
            : base(score, new PlayerConfiguration
            {
                AllowUserInteraction = false,
                AllowFailAnimation = false
            })
        {
            this.score = score;
            this.startTime = startTime;
            this.config = config;
            this.replaying.BindTo(replaying);
            this.noHUD = noHUD;
            priorityScale = Math.Min(config.AspectRatio.Value, 1f / config.AspectRatio.Value);
        }

        protected override void LoadComplete()
        {
            Mods.Value = score.ScoreInfo.Mods;
            base.LoadComplete();

            if (noHUD)
            {
                HUDOverlay.ShowHud.Value = false;
                HUDOverlay.ShowHud.Disabled = true;
                HUDOverlay.PlayfieldSkinLayer.Hide();
                BreakOverlay.Hide();
                DrawableRuleset.Overlays.Hide();
                DrawableRuleset.Playfield.DisplayJudgements.Value = false;
            }

            // Adjust the scale and size of overlays.
            HUDOverlay.ScaleTo(new Vector2(priorityScale));
            HUDOverlay.ResizeTo(new Vector2(1f / priorityScale, 1f / priorityScale));

            BreakOverlay.ScaleTo(new Vector2(priorityScale));
            BreakOverlay.ResizeTo(new Vector2(1f / priorityScale, 1f / priorityScale));

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
