// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class NotificationSettings : SettingsSubsection
    {
        protected override LocalisableString Header => UserInterfaceCustomStrings.NotificationsSectionHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = UserInterfaceCustomStrings.DoNotDisturb,
                    Current = config.GetBindable<bool>(OsuSetting.DoNotDisturb),
                    TooltipText = UserInterfaceCustomStrings.DoNotDisturbDescription,
                    ClassicDefault = false
                },
                new SettingsCheckbox
                {
                    LabelText = UserInterfaceCustomStrings.PersistentNotifications,
                    Current = config.GetBindable<bool>(OsuSetting.PersistentNotifications),
                    TooltipText = UserInterfaceCustomStrings.PersistentNotificationsDescription,
                    ClassicDefault = false,
                }
            };
        }
    }
}
