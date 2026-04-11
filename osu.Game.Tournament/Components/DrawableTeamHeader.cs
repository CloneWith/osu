// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamHeader : TournamentSpriteTextWithBackground
    {
        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly TeamColour colour;

        public DrawableTeamHeader(TeamColour colour)
        {
            this.colour = colour;
            Background.Colour = TournamentGame.GetTeamColour(colour);

            Text.Colour = TournamentGame.TEXT_COLOUR;
            Text.Scale = new Vector2(0.6f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladder.Use1V1Mode.BindValueChanged(use1V1 => Text.Text = use1V1.NewValue
                                                                         ? $"{colour} player".ToUpperInvariant()
                                                                         : $"Team {colour}".ToUpperInvariant(),
                true);
        }
    }
}
