// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableRoundLine : GridContainer
    {
        public readonly TournamentMatch? Match;

        [UsedImplicitly]
        private Bindable<string>? roundName;

        [UsedImplicitly]
        private Bindable<DateTimeOffset>? roundStartTime;

        private readonly DrawableDate dateTimeText;
        private readonly TruncatingSpriteText roundText;

        public DrawableRoundLine(TournamentMatch? match, bool fixedWidth = false, bool monochromeTitle = false)
        {
            Match = match;

            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
            };

            ColumnDimensions = fixedWidth
                ? new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.15f),
                    new Dimension(GridSizeMode.Relative, 0.15f),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                }
                : new[]
                {
                    new Dimension(GridSizeMode.Absolute, 100),
                    new Dimension(GridSizeMode.Absolute, 100),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                };

            Content = new[]
            {
                new Drawable[]
                {
                    dateTimeText = new DrawableDate(match?.Round.Value?.StartDate.Value ?? new DateTimeOffset(DateTime.Now))
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Shadow = false,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold),
                    },
                    roundText = new TruncatingSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = match?.Round.Value?.Name.Value ?? "Unknown Round",
                        Shadow = false,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold),
                    },
                    new DrawableTeamLine(match?.Team1.Value, TeamColour.Red, monochromeTitle, !fixedWidth),
                    new TournamentSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = @"vs",
                        Shadow = false,
                        Font = OsuFont.Torus.With(size: 28, weight: FontWeight.SemiBold),
                        Padding = new MarginPadding
                        {
                            Horizontal = 20
                        },
                    },
                    new DrawableTeamLine(match?.Team2.Value, TeamColour.Blue, monochromeTitle, !fixedWidth),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Match?.Round.Value != null)
            {
                (roundName = Match.Round.Value.Name.GetBoundCopy()).BindValueChanged(r =>
                    roundText.Text = r.NewValue);

                (roundStartTime = Match.Round.Value.StartDate.GetBoundCopy()).BindValueChanged(t =>
                    dateTimeText.Date = t.NewValue);
            }
        }
    }
}
