// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Screens.Footer;
using osu.Game.Tournament.Localisation;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class FooterButtonNew : ScreenFooterButton, IHasPopover, IKeyBindingHandler<PlatformAction>
    {
        /// <summary>
        /// Invoked when a new profile is created via the popover.
        /// </summary>
        public Action<ShowcaseConfig>? OnCreate;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Icon = FontAwesome.Solid.Plus;
            Text = BaseStrings.AddNew;
            AccentColour = colour.Blue1;

            Action = () =>
            {
                if (this.FindClosestParent<PopoverContainer>()?.CurrentTarget == this)
                    this.HidePopover();
                else
                    this.ShowPopover();
            };
        }

        public Popover GetPopover() => new NewProfilePopover(this);

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat)
                return true;

            switch (e.Action)
            {
                case PlatformAction.DocumentNew:
                    TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }
}
