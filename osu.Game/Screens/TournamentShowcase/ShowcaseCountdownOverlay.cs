// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseCountdownOverlay : DelayedResumeOverlay
    {
        public ShowcaseCountdownOverlay(double duration = 2000)
            : base(duration)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Manually change the state of the overlay.
            // Otherwise, the countdown won't be shown properly.
            Show();
            PopIn();
        }
    }
}
