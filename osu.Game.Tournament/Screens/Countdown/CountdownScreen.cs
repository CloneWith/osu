// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Countdown
{
    public partial class CountdownScreen : TournamentScreen
    {
        public Bindable<CountdownState> CurrentState = new Bindable<CountdownState>();
        public readonly BindableBool AutoProgress = new BindableBool();

        private MatchCountdown countdown = null!;

        public enum CountdownState
        {
            CountdownOnly,
            WithSchedule,
            WithUpcoming,
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = @"Main Content",
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        countdown = new MatchCountdown
                        {
                            Name = @"Countdown",
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                },
                new ControlPanel
                {
                    new LabelledSwitchButton
                    {
                        Label = "Auto progress",
                        Current = AutoProgress,
                    },
                    new SectionHeader("Screen Layout"),
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Countdown only",
                        Action = () => CurrentState.Value = CountdownState.CountdownOnly,
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Schedule",
                        Action = () => CurrentState.Value = CountdownState.WithSchedule,
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Upcoming match",
                        Action = () => CurrentState.Value = CountdownState.WithUpcoming,
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentState.BindValueChanged(stateChanged);
        }

        private void stateChanged(ValueChangedEvent<CountdownState> state)
        {
            // TODO: Expand this function
        }

        public override void FirstSelected(bool enforced = false)
        {
            base.FirstSelected(enforced);

            countdown.Target.Value = LadderInfo.CurrentMatch.Value?.Date.Value;
        }
    }
}
