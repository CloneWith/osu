// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class CustomNotificationsStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Notifications";

        /// <summary>
        /// "Hi! This is a customized client for osu! FumoFumo Cup. Read the document for detailed usages and actively report possible bugs."
        /// </summary>
        public static LocalisableString Greeting => new TranslatableString(getKey(@"greeting"),
            @"Hi! This is a customized client for osu! FumoFumo Cup. Read the document for detailed usages and actively report possible bugs.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

