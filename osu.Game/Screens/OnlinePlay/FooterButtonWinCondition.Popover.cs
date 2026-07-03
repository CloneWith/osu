// This file is originally created by GooGuTeam.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonWinCondition
    {
        public partial class Popover : OsuPopover
        {
            private FillFlowContainer buttonFlow = null!;
            private readonly FooterButtonWinCondition footerButton;

            public required OverlayColourProvider ColourProvider { get; init; }

            private BindableBool reversedFreestyle { get; } = new BindableBool(true);

            public Popover(FooterButtonWinCondition footerButton)
            {
                this.footerButton = footerButton;

                reversedFreestyle.Value = !footerButton.Freestyle.Value;

                footerButton.Freestyle.BindValueChanged(e =>
                    {
                        reversedFreestyle.Value = !e.NewValue;
                        if (footerButton.WinCondition.Value is not Online.Multiplayer.WinCondition.Score and not Online.Multiplayer.WinCondition.Accuracy)
                            footerButton.WinCondition.Value = Online.Multiplayer.WinCondition.Score;
                    }
                );
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Content.Padding = new MarginPadding(5);

                Child = buttonFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(3),
                };

                var conditions = Enum.GetValues<WinCondition>();

                foreach (var condition in conditions)
                {
                    buttonFlow.Add(new FooterPopoverButton
                    {
                        Text = condition.GetLocalisableDescription(),
                        Icon = footerButton.WinCondition.Value == condition ? FontAwesome.Solid.Check : new IconUsage(),
                        BackgroundColour = ColourProvider.Background3,
                        TextColour = null,
                        Action = () =>
                        {
                            Scheduler.AddDelayed(Hide, 50);
                            footerButton.WinCondition.Value = condition;
                        },
                        Enabled =
                        {
                            BindTarget = condition is Online.Multiplayer.WinCondition.Score or Online.Multiplayer.WinCondition.Accuracy
                                ? new BindableBool(true)
                                : reversedFreestyle
                        }
                    });
                }
            }
        }
    }
}
