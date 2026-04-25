// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
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
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceCustomStrings.DoNotDisturb,
                    Current = config.GetBindable<bool>(OsuSetting.DoNotDisturb),
                    HintText = UserInterfaceCustomStrings.DoNotDisturbDescription,
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceCustomStrings.PersistentNotifications,
                    Current = config.GetBindable<bool>(OsuSetting.PersistentNotifications),
                    HintText = UserInterfaceCustomStrings.PersistentNotificationsDescription,
                }),
            };
        }
    }
}
