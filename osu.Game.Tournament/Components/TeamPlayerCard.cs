// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tournament.Caching;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TeamPlayerCard : UserPanel
    {
        private readonly APIUser? teamPlayer;

        private Box topMask = null!;
        private RotatingDisplayContainer details = null!;
        private FillFlowContainer statDisplay = null!;
        private FillFlowContainer bombDisplay = null!;
        private TournamentSpriteText punishmentText = null!;

        private decimal? displayedPP;
        private int? displayedRank;
        private bool statisticsDisplayed;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        [Resolved]
        private TournamentUserCache userCache { get; set; } = null!;

        public TeamPlayerCard(APIUser user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;
            CornerRadius = 6;
            teamPlayer = user;
        }

        protected override Drawable CreateBackground() => new TournamentUserCoverBackground
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Colour = Color4.Gray,
            User = User,
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            // Whatever the bracket already holds is drawn straight away. The card is on stream, so showing a
            // possibly stale pp and rank immediately beats showing nothing until the request comes back.
            updateStatistics(User.Statistics);

            userCache.GetUserAsync(User.Id).ContinueWith(t =>
                Scheduler.Add(() => updateStatistics(t.GetResultSafely()?.GetStatisticsFor(ladder.Ruleset.Value))));
        }

        private void updateStatistics(UserStatistics? statistics)
        {
            decimal? pp = statistics?.PP;
            int? rank = statistics?.GlobalRank;

            if (pp == displayedPP && rank == displayedRank)
                return;

            displayedPP = pp;
            displayedRank = rank;

            statDisplay.Children = new Drawable[]
            {
                new TournamentSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = pp == null ? string.Empty : $"{pp}pp",
                    Font = OsuFont.TorusAlternate.With(weight: FontWeight.SemiBold, size: 20),
                    Shadow = true
                },
                new TournamentSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = rank == null ? string.Empty : $"#{rank}",
                    Font = OsuFont.TorusAlternate.With(weight: FontWeight.Medium, size: 17),
                    Shadow = true
                }
            };

            if (statisticsDisplayed)
                return;

            statisticsDisplayed = true;
            statDisplay.FadeInFromZero(duration: 200, easing: Easing.OutCubic);
        }

        protected override Drawable CreateLayout()
        {
            var layout = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(40),
                                Masking = true,
                                CornerRadius = 6,
                                Margin = new MarginPadding { Left = 10 },
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(25),
                                        Icon = FontAwesome.Solid.UserAlt,
                                    },
                                    new TournamentAvatar(teamPlayer)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(40),
                                    },
                                }
                            },
                            CreateUsername().With(username =>
                            {
                                username.Text = username.Text.ToString().TruncateWithEllipsis(20);
                                username.Anchor = Anchor.CentreLeft;
                                username.Origin = Anchor.CentreLeft;
                                username.UseFullGlyphHeight = false;
                                username.Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24);
                            })
                        }
                    },
                    details = new RotatingDisplayContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Both,
                        DisplayLength = 10000,
                        Margin = new MarginPadding { Right = 10 },
                    },
                    topMask = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                        Alpha = 0,
                    },
                },
            };

            details.AddLayers(new Drawable[]
            {
                statDisplay = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                },
                new FillFlowContainer
                {
                    Name = @"Punishment Display",
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(1),
                    Children = new Drawable[]
                    {
                        bombDisplay = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2),
                        },
                        punishmentText = new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Font = OsuFont.Torus.With(size: 17),
                            Text = BaseStrings.Punishment,
                        },
                    },
                },
            });

            return layout;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ladder.Punishments.BindCollectionChanged((_, _) => updatePunishmentDisplay(), true);
        }

        private void updatePunishmentDisplay()
        {
            var punishments = ladder.Punishments.Where(p => !p.IsExpired
                                                            && p.Type.Value is not PunishmentType.Pending
                                                            && p.UserID.Value == User.OnlineID)
                                    .ToList();

            if (punishments.Count != 0)
            {
                int penalty = punishments.Sum(p => p.Penalty.Value);

                bombDisplay.Colour = penalty switch
                {
                    1 => FumoColours.SunshineYellow.Regular,
                    2 => FumoColours.FlandreRed.Regular,
                    _ => FumoColours.DeepPurple.Regular,
                };

                topMask.Alpha = penalty >= TournamentExtensions.PUNISHMENT_THRESHOLD ? 1 : 0;
                punishmentText.Text = penalty >= TournamentExtensions.PUNISHMENT_THRESHOLD ? BaseStrings.Disqualified : BaseStrings.Punishment;

                if (penalty > 3)
                {
                    bombDisplay.Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.Bomb,
                            Size = new Vector2(24),
                        },
                        new TournamentSpriteText
                        {
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                            Text = penalty.ToString(),
                        },
                    };
                }
                else
                {
                    bombDisplay.ChildrenEnumerable = from i in Enumerable.Range(0, penalty)
                                                     select new SpriteIcon
                                                     {
                                                         Icon = FontAwesome.Solid.Bomb,
                                                         Size = new Vector2(24),
                                                     };
                }

                details.Start();
            }
            else
            {
                details.Pause();
                details.ShowIndex(0);
            }
        }
    }
}
