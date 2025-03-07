// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Schedule
{
    public partial class ScheduleScreen : TournamentScreen
    {
        private readonly BindableList<TournamentMatch> allMatches = new BindableList<TournamentMatch>();
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();
        private Container mainContainer = null!;
        private LadderInfo ladder = null!;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            this.ladder = ladder;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Schedule)
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(100) { Bottom = 50 },
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new DrawableTournamentHeaderText(),
                                            new Container
                                            {
                                                Margin = new MarginPadding { Top = 40 },
                                                AutoSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        Colour = Color4.White,
                                                        Size = new Vector2(50, 10),
                                                    },
                                                    new TournamentSpriteTextWithBackground("Schedule")
                                                    {
                                                        X = 60,
                                                        Scale = new Vector2(0.8f)
                                                    }
                                                }
                                            },
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    mainContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            allMatches.BindTo(ladder.Matches);
            allMatches.BindCollectionChanged((_, _) => refresh());

            currentMatch.BindTo(ladder.CurrentMatch);
            currentMatch.BindValueChanged(_ => refresh(), true);
        }

        private void refresh()
        {
            const int days_for_displays = 4;

            IEnumerable<ConditionalTournamentMatch> conditionals =
                allMatches
                    .Where(m => !m.Completed.Value && (m.Team1.Value == null || m.Team2.Value == null) && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .SelectMany(m => m.ConditionalMatches.Where(cp => m.Acronyms.TrueForAll(a => cp.Acronyms.Contains(a))));

            IEnumerable<TournamentMatch> upcoming =
                allMatches
                    .Where(m => !m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .Concat(conditionals)
                    .OrderBy(m => m.Date.Value)
                    .Take(8);

            var recent =
                allMatches
                    .Where(m => m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .OrderByDescending(m => m.Date.Value)
                    .Take(8);

            ScheduleContainer comingUpNext;

            mainContainer.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.74f,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ScheduleContainer("recent matches")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.4f,
                                    ChildrenEnumerable = recent.Select(p => new ScheduleMatch(p))
                                },
                                new ScheduleContainer("upcoming matches")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.6f,
                                    ChildrenEnumerable = upcoming.Select(p => new ScheduleMatch(p))
                                },
                            }
                        }
                    },
                    comingUpNext = new ScheduleContainer("coming up next")
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.25f,
                    }
                }
            };

            if (currentMatch.Value != null)
            {
                comingUpNext.Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(30),
                    Children = new Drawable[]
                    {
                        new ScheduleMatch(currentMatch.Value, false)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new TournamentSpriteTextWithBackground(currentMatch.Value.Round.Value?.Name.Value ?? string.Empty)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Scale = new Vector2(0.5f)
                        },
                        new TournamentSpriteTextWithBackground(text: currentMatch.Value.Team1.Value?.FullName.Value ?? string.Empty,
                            backgroundColor: TournamentGame.COLOUR_RED, textColor: Color4.White,
                            fontSize: 30, textWeight: FontWeight.SemiBold
                        )
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = "vs",
                            Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold)
                        },
                        new TournamentSpriteTextWithBackground(text: currentMatch.Value.Team2.Value?.FullName.Value ?? string.Empty,
                            backgroundColor: TournamentGame.COLOUR_BLUE, textColor: Color4.White,
                            fontSize: 30, textWeight: FontWeight.SemiBold
                        )
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                new ScheduleMatchDate(currentMatch.Value.Date.Value)
                                {
                                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Regular)
                                }
                            }
                        },
                    }
                };
            }
        }

        public partial class ScheduleMatch : DrawableTournamentMatch
        {
            private readonly TournamentMatch match;
            private readonly bool showTimestamp;

            private TournamentSpriteText timeText = null!;

            public ScheduleMatch(TournamentMatch match, bool showTimestamp = true)
                : base(match, interactive: false)
            {
                this.match = match;
                this.showTimestamp = showTimestamp;
                Flow.Direction = FillDirection.Horizontal;

                Scale = new Vector2(0.8f);
            }

            [BackgroundDependencyLoader]
            private void load(LadderInfo ladder)
            {
                bool conditional = match is ConditionalTournamentMatch;

                if (conditional)
                    Colour = OsuColour.Gray(0.5f);

                if (showTimestamp)
                {
                    AddInternal(new DrawableDate(Match.Date.Value)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Font = OsuFont.Torus,
                        Margin = new MarginPadding
                        {
                            Horizontal = 10,
                            Vertical = 5,
                        },
                    });
                    AddInternal(timeText = new TournamentSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Margin = new MarginPadding
                        {
                            Horizontal = 10,
                            Vertical = 5,
                        },
                        Text = (ladder.UseUtcTime.Value
                                   ? match.Date.Value.ToUniversalTime().ToString("HH:mm UTC")
                                   : match.Date.Value.ToLocalTime().ToString("HH:mm"))
                               + (conditional ? " (conditional)" : "")
                    });

                    ladder.UseUtcTime.BindValueChanged(e => timeText.Text = (e.NewValue
                                                                                ? match.Date.Value.ToUniversalTime().ToString("HH:mm UTC")
                                                                                : match.Date.Value.ToLocalTime().ToString("HH:mm"))
                                                                            + (conditional ? " (conditional)" : ""));
                }
            }
        }

        public partial class ScheduleMatchDate : DrawableDate
        {
            public ScheduleMatchDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
                : base(date, textSize, italic)
            {
            }

            protected override string Format() => Date < DateTimeOffset.Now
                ? $"Started {base.Format()}"
                : $"Starting {base.Format()}";
        }

        public partial class ScheduleContainer : Container
        {
            protected override Container<Drawable> Content => content;

            private readonly FillFlowContainer content;

            public ScheduleContainer(string title)
            {
                Padding = new MarginPadding { Left = 60, Top = 10 };
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteTextWithBackground(text: title.ToUpperInvariant(),
                                backgroundColor: Color4.White.Opacity(0), textColor: Color4.White
                            )
                            {
                                Scale = new Vector2(0.5f)
                            },
                            content = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                RelativeSizeAxes = Axes.Both,
                                Spacing = new Vector2(0, -6),
                                Margin = new MarginPadding(10)
                            },
                        }
                    },
                };
            }
        }
    }
}
