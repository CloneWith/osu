// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamPlayerList : FillFlowContainer
    {
        private int totalHeight;
        private const int entryheight = 50;
        private int internalSpacing;
        private float cornerRadius;

        public DrawableTeamPlayerList(TournamentTeam? team, float cornerRadius = 6, int spacing = 10)
        {
            internalSpacing = spacing;
            this.cornerRadius = cornerRadius;

            var players = team?.Players ?? new BindableList<TournamentUser>();

            // AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.None;
            Width = 300;
            Direction = FillDirection.Vertical;
            totalHeight = players.Count * (entryheight + internalSpacing) + internalSpacing;
            Height = totalHeight;
            // We need to provide all children upon definition of a widget,
            // Since it's impossible to change its height after that.
            ChildrenEnumerable = players.Select(createCard);
            this.cornerRadius = cornerRadius;
        }
        private TeamPlayerCard createCard(TournamentUser user) => new TeamPlayerCard(user.ToAPIUser())
        {
            RelativeSizeAxes = Axes.None,
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            Width = 300,
            Height = entryheight,
            Margin = new MarginPadding { Bottom = internalSpacing },
            Scale = new Vector2(1f),
            CornerRadius = cornerRadius,
            // BackgroundColour = ColourInfo.GradientHorizontal(Color4.White.Opacity(1), Color4.White.Opacity(0.7f)),
        };

        public int GetHeight() => totalHeight;

        public void ReloadWithTeam(TournamentTeam? team)
        {
            var players = team?.Players ?? new BindableList<TournamentUser>();
            totalHeight = players.Count * (entryheight + internalSpacing) + internalSpacing;
            ChildrenEnumerable = players.Select(createCard);
            this.ResizeHeightTo(totalHeight, 500, Easing.OutCubic);
        }
    }
}
