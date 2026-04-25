// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class UserInterfaceCustomStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.UserInterface";

        /// <summary>
        /// "Notifications"
        /// </summary>
        public static LocalisableString NotificationsSectionHeader => new TranslatableString(getKey(@"notifications_section_header"), @"Notifications");

        /// <summary>
        /// "Do not disturb"
        /// </summary>
        public static LocalisableString DoNotDisturb => new TranslatableString(getKey(@"do_not_disturb"), @"Do not disturb");

        /// <summary>
        /// "Notifications will never pop up."
        /// </summary>
        public static LocalisableString DoNotDisturbDescription => new TranslatableString(getKey(@"do_not_disturb_description"),
            @"Notifications will never pop up.");

        /// <summary>
        /// "Persistent notifications"
        /// </summary>
        public static LocalisableString PersistentNotifications => new TranslatableString(getKey(@"persistent_notifications"), @"Persistent notifications");

        /// <summary>
        /// "Unless removed manually, all notifications will stay in the notification area."
        /// </summary>
        public static LocalisableString PersistentNotificationsDescription => new TranslatableString(getKey(@"persistent_notifications_description"),
            @"Unless removed manually, all notifications will stay in the notification area.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
