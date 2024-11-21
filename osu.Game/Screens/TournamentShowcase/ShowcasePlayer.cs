// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcasePlayer : ReplayPlayer
    {
        private readonly ShowcaseConfig config;
        private readonly double startTime;

        private readonly BindableBool replaying = new BindableBool();

        public ShowcasePlayer(Score score, double startTime, ShowcaseConfig config, BindableBool replaying)
            : base(score, new PlayerConfiguration
            {
                AllowUserInteraction = false,
                AllowFailAnimation = false
            })
        {
            this.startTime = startTime;
            this.config = config;
            this.replaying.BindTo(replaying);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Reset();
        }

        protected override void Update()
        {
            base.Update();

            if (GameplayState.HasPassed)
            {
                replaying.Value = false;
            }
        }

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
