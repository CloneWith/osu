// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class MatchRoundDisplay : FumoSpriteText
    {
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        [Resolved]
        private TournamentThemeProvider themeProvider { get; set; } = null!;

        public MatchRoundDisplay()
        {
            BorderColour = Color4.White;
            BorderThickness = 3;
            Text.Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 55);
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged, true);
            currentMatch.BindTo(ladder.CurrentMatch);

            themeProvider.Current.BindValueChanged(themeChanged, true);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            Text.Text = match.NewValue?.Round.Value?.Name.Value ?? BaseStrings.UnknownRound;
        }

        private void themeChanged(ValueChangedEvent<RoundTheme> theme)
        {
            BackgroundColour = theme.NewValue.Accent;
            TextColour = OsuColour.ForegroundTextColourFor(BackgroundColour);
        }
    }
}
