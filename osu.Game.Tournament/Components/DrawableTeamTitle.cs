// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamTitle : TournamentSpriteTextWithBackground
    {
        private readonly TournamentTeam? team;

        [UsedImplicitly]
        private Bindable<string>? acronym;

        public DrawableTeamTitle(TournamentTeam? team, bool noBackground = false, TeamColour colour = TeamColour.Neutral)
        {
            this.team = team;
            Background.Alpha = noBackground ? 0 : 1;

            Text.Text = "???";
            Text.Colour = colour switch
            {
                TeamColour.Red => TournamentGame.COLOUR_RED,
                TeamColour.Blue => TournamentGame.COLOUR_BLUE,
                _ => noBackground ? Color4.White : Color4.Black,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (team == null) return;

            (acronym = team.Acronym.GetBoundCopy()).BindValueChanged(_ => Text.Text = team?.FullName.Value ?? "???", true);
        }
    }
}
