// This file is originally created by GooGuTeam.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonWinCondition : ScreenFooterButton, IHasPopover
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public readonly Bindable<WinCondition> WinCondition = new Bindable<WinCondition>();
        public readonly Bindable<bool> Freestyle = new Bindable<bool>();

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = OnlinePlayStrings.FooterButtonWinCondition;
            Icon = FontAwesome.Solid.Medal;
            AccentColour = colours.Orange1;

            Action = () =>
            {
                if (this.FindClosestParent<PopoverContainer>()?.CurrentTarget == this)
                    this.HidePopover();
                else
                    this.ShowPopover();
            };
        }

        public Framework.Graphics.UserInterface.Popover GetPopover() => new Popover(this)
        {
            ColourProvider = colourProvider
        };
    }
}
