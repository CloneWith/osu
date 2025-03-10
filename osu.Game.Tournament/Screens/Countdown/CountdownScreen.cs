// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Countdown
{
    public partial class CountdownScreen : TournamentScreen
    {
        private readonly BindableBool autoProgress = new BindableBool();
        private readonly BindableBool showSchedule = new BindableBool();
        private readonly BindableBool showUpcoming = new BindableBool();

        private MatchCountdown countdown = null!;
        private ReverseChildIDFillFlowContainer<Drawable> upcomingContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var upcomingMatch = LadderInfo.CurrentMatch.Value;

            InternalChildren = new Drawable[]
            {
                countdown = new MatchCountdown
                {
                    Name = @"Countdown",
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -0.75f,
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
                                Text = "Upcoming Match",
                                Colour = Color4.Black,
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                                Shadow = false,
                            },
                        },
                        new CustomRoundedBox
                        {
                            BackgroundColour = FumoColours.SeaBlue.Regular,
                            Child = new DrawableRoundLine(upcomingMatch)
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
                new ControlPanel
                {
                    new LabelledSwitchButton
                    {
                        Label = "Auto progress",
                        Current = autoProgress,
                    },
                    new SectionHeader("Screen Layout"),
                    new LabelledSwitchButton
                    {
                        Label = "Show schedule",
                        Current = showSchedule,
                    },
                    new LabelledSwitchButton
                    {
                        Label = "Show upcoming",
                        Current = showUpcoming,
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showUpcoming.BindValueChanged(v =>
                upcomingContainer.MoveToY(v.NewValue ? -20 : 100, 500, Easing.InOutQuint));
        }

        public override void FirstSelected(bool enforced = false)
        {
            base.FirstSelected(enforced);

            countdown.Target.Value = LadderInfo.CurrentMatch.Value?.Date.Value;
            countdown.Delay(700).MoveToY(0, 1000, Easing.OutQuint);
        }
    }
}
