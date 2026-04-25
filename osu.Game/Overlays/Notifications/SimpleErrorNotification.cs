// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;

namespace osu.Game.Overlays.Notifications
{
    public partial class SimpleErrorNotification : SimpleNotification
    {
        public override string PopInSampleName => @"UI/notification-error";

        private readonly bool isCritical;

        public SimpleErrorNotification(bool isCritical = false)
        {
            this.isCritical = isCritical;

            Icon = isCritical ? FontAwesome.Solid.Bomb : FontAwesome.Solid.ExclamationTriangle;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            IconContent.Colour = isCritical ? FumoColours.FlandreRed.Regular : FumoColours.SunshineYellow.Regular;
            Light.Colour = isCritical ? FumoColours.FlandreRed.Regular : FumoColours.SunshineYellow.Regular;
        }
    }
}
