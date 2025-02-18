// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using Humanizer;
using Humanizer.Localisation;

namespace osu.Game.Tournament
{
    /// <summary>
    /// Utilities for time manipulation, which is especially useful in scheduling.
    /// </summary>
    public static class TimeExtensions
    {
        /// <summary>
        /// Turn a <see cref="TimeSpan"/> struct into a string representation respecting current culture.
        /// </summary>
        /// <param name="timeSpan">the <see cref="TimeSpan"/> to be handled</param>
        /// <param name="useChineseCharacters">force to use Chinese representation of time (e.g. 1 minute -> 1 分钟)</param>
        /// <returns>a string representation of the time</returns>
        /// <remarks>Since Humanizer won't return fully translated time strings, we need to add an argument to handle this by ourselves.</remarks>
        public static string ToHumanizedString(this TimeSpan timeSpan, bool useChineseCharacters = true)
        {
            string humanizedTime = timeSpan.Humanize(culture: CultureInfo.CurrentUICulture, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second);

            if (useChineseCharacters)
            {
                return humanizedTime.Replace("seconds", "second").Replace("minutes", "minute").Replace("seconds", "second").Replace("hours", "hour")
                                    .Replace("days", "day").Replace("months", "month").Replace("weeks", "week").Replace("years", "year")
                                    .Replace("second", "秒").Replace("minute", "分钟").Replace("hour", "小时").Replace("day", "天")
                                    .Replace("week", "周").Replace("year", "年");
            }

            return humanizedTime;
        }

        /// <summary>
        /// Check if a span of time is nearly zero, useful for checking if two time points are almost the same.
        /// </summary>
        /// <param name="timeSpan">the span of time</param>
        /// <param name="precision">allowable time difference, in milliseconds (only used when <paramref name="allowEarlier"/> is true)</param>
        /// <param name="allowEarlier">allow positive time span</param>
        /// <returns>true is nearly equals to zero, otherwise false</returns>
        public static bool NearlyEqualsZero(this TimeSpan timeSpan, int precision = 10, bool allowEarlier = false)
        {
            return allowEarlier
                ? timeSpan.TotalMilliseconds <= precision
                : timeSpan.TotalMilliseconds <= 0;
        }
    }
}
