// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public static class PunishmentEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.PunishmentEditor";

        /// <summary>
        /// "Reason"
        /// </summary>
        public static LocalisableString Reason => new TranslatableString(getKey(@"reason"), @"Reason");

        /// <summary>
        /// "Penalty point"
        /// </summary>
        public static LocalisableString Penalty => new TranslatableString(getKey(@"penalty"), @"Penalty point");

        /// <summary>
        /// "Punishment type"
        /// </summary>
        public static LocalisableString PunishmentType => new TranslatableString(getKey(@"punishment_type"), @"Punishment type");

        /// <summary>
        /// "Record time"
        /// </summary>
        public static LocalisableString RecordTime => new TranslatableString(getKey(@"record_time"), @"Record time");

        /// <summary>
        /// "Expire time"
        /// </summary>
        public static LocalisableString ExpireTime => new TranslatableString(getKey(@"expire_time"), @"Expire time");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

