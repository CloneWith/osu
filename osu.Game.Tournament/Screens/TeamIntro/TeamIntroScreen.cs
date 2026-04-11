// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamIntro
{
    public partial class TeamIntroScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;

        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("teamintro")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            mainContainer.Clear();

            if (match.NewValue == null)
                return;

            const float y_flag_offset = 292;

            const float y_offset = 460;

            Drawable team1Display = ladderInfo.Use1V1Mode.Value
                                        ? new DrawableTeamTitleWithHeader(match.NewValue.Team1.Value, TeamColour.Red)
                                        : new DrawableTeamWithPlayers(match.NewValue.Team1.Value, TeamColour.Red);
            Drawable team2Display = ladderInfo.Use1V1Mode.Value
                                        ? new DrawableTeamTitleWithHeader(match.NewValue.Team2.Value, TeamColour.Blue)
                                        : new DrawableTeamWithPlayers(match.NewValue.Team2.Value, TeamColour.Blue);

            team1Display.Position = new Vector2(165, y_offset);
            team2Display.Position = new Vector2(740, y_offset);

            mainContainer.Children = new[]
            {
                new RoundDisplay(match.NewValue)
                {
                    Position = new Vector2(100, 100)
                },
                new DrawableTeamFlag(match.NewValue.Team1.Value)
                {
                    Position = new Vector2(165, y_flag_offset),
                },
                team1Display,
                new DrawableTeamFlag(match.NewValue.Team2.Value)
                {
                    Position = new Vector2(740, y_flag_offset),
                },
                team2Display,
            };
        }
    }
}
