// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class MatchRoundDisplay : FumoSpriteText
    {
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public MatchRoundDisplay()
        {
            BorderColour = Color4.White;
            BackgroundColour = FumoColours.SeaBlue.Regular;
            TextColour = Color4.White;
            BorderThickness = 3;
            Text.Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 55);
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match) =>
            Text.Text = match.NewValue?.Round.Value?.Name.Value ?? BaseStrings.UnknownRound;
    }
}
