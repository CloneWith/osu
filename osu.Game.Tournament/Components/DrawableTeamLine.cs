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
        public DrawableTeamLine(TournamentTeam? team, TeamColour colour,
                                bool monochromeTitle = false, bool autoSizeX = true)
        {
            var anchor = colour == TeamColour.Red ? Anchor.CentreRight : Anchor.CentreLeft;

            Anchor = anchor;
            Origin = anchor;
            RelativeSizeAxes = autoSizeX ? Axes.None : Axes.X;
            AutoSizeAxes = autoSizeX ? Axes.Both : Axes.Y;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(6, 0);
            Children = new Drawable[]
            {
                new FumoTeamFlagDisplay(team, anchor),
                new FumoSeedDisplay(team, colour, anchor),
                new FumoTeamTitleDisplay(team, monochromeTitle ? TeamColour.Neutral : colour, anchor, !autoSizeX),
            };
        }

        public partial class FumoTeamFlagDisplay : DrawableTeamFlag
        {
            public FumoTeamFlagDisplay(TournamentTeam? team, Anchor anchor = Anchor.Centre)
                : base(team)
            {
                Anchor = anchor;
                Origin = anchor;
                Scale = new Vector2(0.75f);
            }
        }

        public partial class FumoTeamTitleDisplay : DrawableTeamTitle
        {
            public FumoTeamTitleDisplay(TournamentTeam? team, TeamColour colour = TeamColour.Neutral,
                                        Anchor anchor = Anchor.Centre, bool truncate = false)
                : base(team, true, colour, truncate)
            {
                Anchor = anchor;
                Origin = anchor;
                Text.Padding = new MarginPadding();
                Text.Shadow = false;
                Text.Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold);
            }
        }

        public partial class FumoSeedDisplay : DrawableTeamSeed
        {
            public FumoSeedDisplay(TournamentTeam? team, TeamColour colour = TeamColour.Neutral,
                                   Anchor anchor = Anchor.Centre)
                : base(team, false, colour)
            {
                Anchor = anchor;
                Origin = anchor;
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
