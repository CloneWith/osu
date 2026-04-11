// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    public partial class TournamentSpriteText : OsuSpriteText
    {
        [Resolved(CanBeNull = true)]
        private LadderInfo? ladderInfo { get; set; }

        public TournamentSpriteText()
        {
            Font = OsuFont.Torus;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // only apply override if not default colour
            if (Colour == Color4.White)
                ladderInfo?.TextForegroundColour.BindValueChanged(vce => Colour = vce.NewValue);
        }
    }
}
