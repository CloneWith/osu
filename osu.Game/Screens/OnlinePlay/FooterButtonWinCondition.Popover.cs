// This file is originally created by GooGuTeam.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
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

            public Popover(FooterButtonWinCondition footerButton)
            {
                this.footerButton = footerButton;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
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
                    });
                }
            }
        }
    }
}
