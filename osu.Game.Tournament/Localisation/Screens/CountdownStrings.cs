// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class CountdownStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.Countdown";

        /// <summary>
        /// "Upcoming Match"
        /// </summary>
        public static LocalisableString UpcomingHeader => new TranslatableString(getKey(@"upcoming_header"), @"Upcoming Match");

        /// <summary>
        /// "Recent Matches"
        /// </summary>
        public static LocalisableString RecentHeader => new TranslatableString(getKey(@"recent_header"), @"Recent Matches");

        /// <summary>
        /// "Screen Layout"
        /// </summary>
        public static LocalisableString LayoutSettingHeader => new TranslatableString(getKey(@"layout_setting_header"), @"Screen Layout");

        /// <summary>
        /// "Show schedule"
        /// </summary>
        public static LocalisableString ShowSchedule => new TranslatableString(getKey(@"show_schedule"), @"Show schedule");

        /// <summary>
        /// "Show upcoming"
        /// </summary>
        public static LocalisableString ShowUpcoming => new TranslatableString(getKey(@"show_upcoming"), @"Show upcoming");

        /// <summary>
        /// "Long time to go..."
        /// </summary>
        public static LocalisableString LongWaitingPrompt => new TranslatableString(getKey(@"long_waiting_prompt"), @"Long time to go...");

        /// <summary>
        /// "Waiting for nothing..."
        /// </summary>
        public static LocalisableString EmptyTimePrompt => new TranslatableString(getKey(@"empty_time_prompt"), @"Waiting for nothing...");

        /// <summary>
        /// "Time to go!"
        /// </summary>
        public static LocalisableString EndedPrompt => new TranslatableString(getKey(@"ended_prompt"), @"Time to go!");

        /// <summary>
        /// "Started {0}"
        /// </summary>
        public static LocalisableString JustEndedPrompt(LocalisableString time) => new TranslatableString(getKey(@"just_ended_prompt"), @"Started {0}", time);

        /// <summary>
        /// "Started long ago..."
        /// </summary>
        public static LocalisableString LongEndedPrompt => new TranslatableString(getKey(@"long_ended_prompt"), @"Started long ago...");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

