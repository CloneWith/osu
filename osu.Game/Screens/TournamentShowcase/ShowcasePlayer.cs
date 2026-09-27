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
        public double StartTime { get; init; }

        public BindableBool Replaying { get; } = new BindableBool();

        public ShowcasePlayerDisplayMode DisplayMode { get; init; } = ShowcasePlayerDisplayMode.Normal;

        private readonly Score score;
        private readonly ShowcaseConfig config;
        private readonly ShowcaseBeatmap beatmap;
        private readonly IReadOnlyList<Mod>? mods;

        private readonly float priorityScale;
        private bool faulted;

        private TournamentOriginalBadge originalBadge = null!;

        public event Action<Exception>? OnError;

        public ShowcasePlayer(Score score, ShowcaseConfig config, ShowcaseBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            : base(score, new PlayerConfiguration
            {
                AllowUserInteraction = false,
            })
        {
            this.score = score;
            this.mods = mods;
            this.beatmap = beatmap;
            this.config = config;
            priorityScale = Math.Min(config.AspectRatio.Value, 1f / config.AspectRatio.Value);
        }

        protected override void LoadComplete()
        {
            Mods.Value = mods ?? score.ScoreInfo.Mods;
            base.LoadComplete();

            if (DisplayMode >= ShowcasePlayerDisplayMode.Secret)
            {
                HUDOverlay.ShowHud.Value = false;
                HUDOverlay.ShowHud.Disabled = true;
                DrawableRuleset.Overlays.Hide();
                HUDOverlay.PlayfieldSkinLayer.Hide();
            }

            if (DisplayMode == ShowcasePlayerDisplayMode.None)
            {
                FailOverlay.Hide();
                BreakOverlay.Hide();
                DrawableRuleset.Playfield.DisplayJudgements.Value = false;
            }

            // Adjust the scale and size of overlays.
            HUDOverlay.TopRightElements.AddRange(new Drawable[]
            {
                originalBadge = new TournamentOriginalBadge(@$"{config.TournamentName}/original-badge", config.ColourScheme.Value)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Scale = new Vector2(0.8f),
                    Alpha = beatmap.IsOriginal.Value ? 1 : 0,
                },
                new ShowcaseUserCreditsWedge(beatmap)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Alpha = beatmap.IsOriginal.Value ? 1 : 0,
                },
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

            try
            {
                if (!faulted && GameplayState.HasPassed)
                {
                    Replaying.Value = false;
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                faulted = true;
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
            SetGameplayStartTime(StartTime);
            GameplayClockContainer.Start();
            Replaying.Value = true;
            this.FadeIn(200, Easing.In);
        }
    }

    public enum ShowcasePlayerDisplayMode
    {
        /// <summary>
        /// The regular replay display mode.
        /// </summary>
        Normal,

        /// <summary>
        /// The HUD overlay would be hidden.
        /// </summary>
        Secret,

        /// <summary>
        /// Only the core playfield would be displayed.
        /// </summary>
        None,
    }
}
