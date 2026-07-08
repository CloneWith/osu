// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osu.Game.Utils;

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

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        public TeamPlayerCard(APIUser user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;
            CornerRadius = 6;
            teamPlayer = user;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Origin = Anchor.CentreRight;
            Background.Anchor = Anchor.CentreRight;
            Background.Colour = Color4.Gray;

            var request = new GetUserRequest(userId: User.Id, ruleset: ladder.Ruleset.Value);

            request.Success += user =>
            {
                Scheduler.Add(() =>
                {
                    statDisplay.Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Text = $"{user.Statistics.PP}pp",
                            Font = OsuFont.TorusAlternate.With(weight: FontWeight.SemiBold, size: 20),
                            Shadow = true
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Text = $"#{user.Statistics.GlobalRank}",
                            Font = OsuFont.TorusAlternate.With(weight: FontWeight.Medium, size: 17),
                            Shadow = true
                        }
                    };

                    statDisplay.FadeInFromZero(duration: 200, easing: Easing.OutCubic);
                });
            };

            api.Queue(request);
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
                                    new UpdateableAvatar(user: teamPlayer)
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
            var punishments = ladder.Punishments.Where(p => !p.IsExpired && p.Type.Value is not PunishmentType.Pending
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
