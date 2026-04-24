// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class FumoUserCard : CompositeDrawable
    {
        private readonly APIUser apiUser;
        private readonly TournamentUser user;

        private readonly Color4 accentColour;

        private readonly Bindable<UserStatistics?> statistics = new Bindable<UserStatistics?>();

        public FumoUserCard(TournamentUser user, TeamColour colour)
        {
            ArgumentNullException.ThrowIfNull(user);

            this.user = user;
            apiUser = user.ToAPIUser();
            accentColour = TournamentExtensions.GetTeamColour(colour);

            Size = new Vector2(360, 100);
        }

        private OsuSpriteText globalRankDisplay = null!;

        private LocalisableString globalRank;

        [BackgroundDependencyLoader]
        private void load()
        {
            // Size = new Vector2(320, 240);
            Masking = true;
            CornerRadius = 10;
            BorderColour = accentColour;
            BorderThickness = 2;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                    Colour = Color4.Black,
                },
                new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            new UpdateableAvatar(apiUser, false)
                            {
                                Name = @"Avatar",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(72),
                                Masking = true,
                                CornerRadius = 5,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(3),
                                Margin = new MarginPadding { Left = 15 },
                                Children = new Drawable[]
                                {
                                    new TruncatingSpriteText
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Font = OsuFont.GetFont(size: 32, weight: FontWeight.Bold),
                                        Shadow = false,
                                        Text = apiUser.Username,
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(10),
                                        Children = new Drawable[]
                                        {
                                            new SpriteIcon
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Size = new Vector2(24),
                                                Colour = accentColour,
                                                Icon = user.Role switch
                                                {
                                                    UserRole.Member => FontAwesome.Solid.User,
                                                    UserRole.Strategist => FontAwesome.Solid.Chess,
                                                    UserRole.Leader => FontAwesome.Solid.Flag,
                                                    _ => throw new ArgumentOutOfRangeException(),
                                                },
                                            },
                                            new TruncatingSpriteText
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                RelativeSizeAxes = Axes.X,
                                                Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold),
                                                Colour = accentColour,
                                                Shadow = false,
                                                Text = user.Role switch
                                                {
                                                    UserRole.Member => BaseStrings.TeamMember,
                                                    UserRole.Strategist => BaseStrings.TeamStrategist,
                                                    UserRole.Leader => BaseStrings.TeamLeader,
                                                    _ => throw new ArgumentOutOfRangeException(),
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                            globalRankDisplay = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold),
                                Shadow = false,
                            },
                        ],
                    },
                },
                new SpriteIcon
                {
                    Name = @"Badge icon",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomCentre,
                    Rotation = -30,
                    X = -5,
                    Size = new Vector2(64),
                    Colour = accentColour,
                    Icon = user.Role switch
                    {
                        UserRole.Member => FontAwesome.Regular.User,
                        UserRole.Strategist => FontAwesome.Solid.ChessBoard,
                        UserRole.Leader => FontAwesome.Regular.Flag,
                        _ => throw new ArgumentOutOfRangeException(),
                    },
                    Alpha = 0.3f,
                },
            };

            statistics.Value = apiUser.Statistics;
            statistics.BindValueChanged(stats =>
            {
                globalRank = stats.NewValue?.GlobalRank?.ToLocalisableString("\\##,##0") ?? "-";
                globalRankDisplay.Text = globalRank;
            }, true);
        }
    }
}
