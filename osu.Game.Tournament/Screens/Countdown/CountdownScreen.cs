// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Countdown
{
    public partial class CountdownScreen : TournamentScreen
    {
        private readonly BindableList<TournamentMatch> allMatches = new BindableList<TournamentMatch>();
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private readonly BindableBool showSchedule = new BindableBool();
        private readonly BindableBool showUpcoming = new BindableBool();

        private Container countdownContainer = null!;
        private Sprite kanbanSprite = null!;
        private MatchCountdown countdown = null!;
        private ReverseChildIDFillFlowContainer<Drawable> upcomingContainer = null!;
        private ReverseChildIDFillFlowContainer<Drawable> recentContainer = null!;
        private FillFlowContainer scheduleFlow = null!;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            var upcomingMatch = LadderInfo.CurrentMatch.Value;

            InternalChildren = new Drawable[]
            {
                countdownContainer = new Container
                {
                    Name = @"Countdown container",
                    RelativePositionAxes = Axes.Both,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -0.75f,
                    Children = new Drawable[]
                    {
                        countdown = new MatchCountdown
                        {
                            Name = @"Countdown",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        kanbanSprite = new Sprite
                        {
                            Name = @"Kanban sprite",
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Width = 0.5f,
                            Y = -110,
                            Texture = textures.Get(@"Icons/kanban"),
                            FillMode = FillMode.Fit,
                        },
                    },
                },
                upcomingContainer = new ReverseChildIDFillFlowContainer<Drawable>
                {
                    Name = @"Upcoming container",
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, -5),
                    X = 20,
                    Y = 100,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new CustomRoundedBox
                        {
                            BackgroundColour = Color4.White,
                            // TODO: Implementation in CustomRoundedBox
                            // BorderColour = FumoColours.SeaBlue.Regular,
                            // BorderThickness = 3,
                            Child = new TournamentSpriteText
                            {
                                Text = CountdownStrings.UpcomingHeader,
                                Colour = Color4.Black,
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                                Shadow = false,
                            },
                        },
                        new CustomRoundedBox
                        {
                            BackgroundColour = FumoColours.SeaBlue.Regular,
                            Child = new DrawableRoundLine(upcomingMatch, monochromeTitle: true)
                            {
                                AutoSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Horizontal = 10,
                                    Vertical = 3,
                                },
                            },
                        },
                    },
                },
                recentContainer = new ReverseChildIDFillFlowContainer<Drawable>
                {
                    Name = @"Recent container",
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, -5),
                    X = 900,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new CustomRoundedBox
                        {
                            BackgroundColour = Color4.White,
                            Child = new TournamentSpriteText
                            {
                                Text = CountdownStrings.RecentHeader,
                                Colour = Color4.Black,
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                                Shadow = false,
                            },
                        },
                        new Container
                        {
                            Width = 800,
                            Height = 450,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("#2b2d30"),
                                },
                                scheduleFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Top = 10, Horizontal = 15 },
                                    Spacing = new Vector2(0, 5),
                                },
                            },
                        },
                    },
                },
                new ControlPanel
                {
                    new SectionHeader(CountdownStrings.LayoutSettingHeader),
                    new LabelledSwitchButton
                    {
                        Label = CountdownStrings.ShowSchedule,
                        Current = showSchedule,
                    },
                    new LabelledSwitchButton
                    {
                        Label = CountdownStrings.ShowUpcoming,
                        Current = showUpcoming,
                    },
                },
            };
        }

        private void refresh()
        {
            const int days_for_displays = 4;

            var recent =
                allMatches
                    .Where(m => m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .OrderByDescending(m => m.Date.Value)
                    .Take(8);

            scheduleFlow.ChildrenEnumerable = recent.Select(p => new DrawableRoundLine(p, true)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            allMatches.BindTo(LadderInfo.Matches);
            allMatches.BindCollectionChanged((_, _) => refresh());

            currentMatch.BindTo(LadderInfo.CurrentMatch);
            currentMatch.BindValueChanged(_ => refresh(), true);

            showSchedule.BindValueChanged(v =>
            {
                countdownContainer.MoveToX(v.NewValue ? -0.3f : 0, 1000, Easing.InOutQuint);
                recentContainer.FadeTo(v.NewValue ? 1 : 0, 1000, Easing.InOutQuint);
                recentContainer.MoveToX(v.NewValue ? -30 : 900, 1000, Easing.InOutQuint);
            });

            showUpcoming.BindValueChanged(v =>
            {
                upcomingContainer.FadeTo(v.NewValue ? 1 : 0, 1000, Easing.InOutQuint);
                upcomingContainer.MoveToY(v.NewValue ? -20 : 100, 1000, Easing.InOutQuint);
            });
        }

        public override void FirstSelected(bool enforced = false)
        {
            base.FirstSelected(enforced);

            countdown.Target.Value = LadderInfo.CurrentMatch.Value?.Date.Value;
            countdownContainer.Delay(700).MoveToY(0, 1000, Easing.OutQuint);

            using (BeginDelayedSequence(100))
            {
                kanbanSprite.FadeIn(300, Easing.OutQuint);
                kanbanSprite.MoveToY(-110, 8500, Easing.InOutCubic)
                            .Then()
                            .MoveToY(-130, 8500, Easing.InOutCubic)
                            .Loop();

                kanbanSprite.RotateTo(-0.8f, 7200, Easing.InOutSine)
                            .Then()
                            .RotateTo(0.8f, 7200, Easing.InOutSine)
                            .Loop();
            }
        }
    }
}
