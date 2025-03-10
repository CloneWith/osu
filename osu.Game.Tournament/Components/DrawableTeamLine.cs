// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamLine : FillFlowContainer
    {
        public DrawableTeamLine(TournamentTeam? team, TeamColour colour)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(6, 0),
                Children = colour == TeamColour.Red
                    ? new Drawable[]
                    {
                        new FumoTeamTitleDisplay(team),
                        new FumoSeedDisplay(team, TeamColour.Red),
                        new FumoTeamFlagDisplay(team),
                    }
                    : new Drawable[]
                    {
                        new FumoTeamFlagDisplay(team),
                        new FumoSeedDisplay(team, TeamColour.Blue),
                        new FumoTeamTitleDisplay(team),
                    },
            };
        }

        public partial class FumoTeamFlagDisplay : DrawableTeamFlag
        {
            public FumoTeamFlagDisplay(TournamentTeam? team)
                : base(team)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Scale = new Vector2(0.75f);
            }
        }

        public partial class FumoTeamTitleDisplay : DrawableTeamTitle
        {
            public FumoTeamTitleDisplay(TournamentTeam? team, TeamColour colour = TeamColour.Neutral)
                : base(team, true, colour)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Text.Padding = new MarginPadding();
                Text.Shadow = false;
                Text.Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold);
            }
        }

        public partial class FumoSeedDisplay : DrawableTeamSeed
        {
            public FumoSeedDisplay(TournamentTeam? team, TeamColour colour = TeamColour.Neutral)
                : base(team, false, colour)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 5;
                Text.Padding = new MarginPadding(3);
                Text.Shadow = false;
                Text.Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold);
            }
        }
    }
}
