// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamSeed : TournamentSpriteTextWithBackground
    {
        private readonly TournamentTeam? team;

        private IBindable<string> seed = null!;
        private Bindable<bool> displaySeed = null!;

        public DrawableTeamSeed(TournamentTeam? team, bool noBackground = true, TeamColour colour = TeamColour.Neutral)
        {
            this.team = team;
            Background.Alpha = noBackground ? 0 : 1;

            Text.Text = "#?";
            Text.Font = Text.Font.With(size: 36);
            Text.Colour = colour switch
            {
                TeamColour.Red => TournamentGame.COLOUR_RED,
                TeamColour.Blue => TournamentGame.COLOUR_BLUE,
                _ => noBackground ? Color4.White : Color4.Black,
            };
        }

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (team == null)
                return;

            seed = team.Seed.GetBoundCopy();
            seed.BindValueChanged(s => Text.Text = s.NewValue, true);

            displaySeed = ladder.DisplayTeamSeeds.GetBoundCopy();
            displaySeed.BindValueChanged(v => Alpha = v.NewValue ? 1 : 0, true);
        }
    }
}
