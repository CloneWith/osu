// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseConfigScreen : OsuScreen
    {
        public ShowcaseConfigScreen()
        {
            Alpha = 0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(500, Easing.OutQuint);
        }
    }
}
