// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Punishment information for a <see cref="TournamentUser"/>.
    /// </summary>
    [Serializable]
    public class PunishmentInfo
    {
        /// <summary>
        /// The maximum allowable penalty points. Users with points more than this would be disqualified.
        /// </summary>
        public const int RESTRICT_THRESHOLD = 3;

        /// <summary>
        /// A list that stores all punishments of a user.
        /// </summary>
        public List<PunishmentEntry> Records = new List<PunishmentEntry>();

        /// <summary>
        /// Currently valid penalties for a user.
        /// </summary>
        public int TotalPenalties => Records.Where(r => r.Type != PunishmentType.Pending && !r.IsExpired)
                                            .Sum(r => r.Penalty);

        /// <summary>
        /// Whether the user should be considered as disqualified now.
        /// </summary>
        public bool IsDisqualified => TotalPenalties >= RESTRICT_THRESHOLD || Records.Any(r => r.Type == PunishmentType.Fatal);
    }

    /// <summary>
    /// Represents a single punishment record.
    /// </summary>
    [Serializable]
    public class PunishmentEntry
    {
        /// <summary>
        /// The time of the punishment being decided / set.
        /// </summary>
        /// <remarks>This property only can be set at definition time.</remarks>
        public DateTimeOffset RecordTime { get; init; } = DateTimeOffset.Now;

        /// <summary>
        /// The type of the punishment.
        /// </summary>
        public PunishmentType Type { get; set; } = PunishmentType.Normal;

        /// <summary>
        /// Amount of warning points one gets for the punishment.
        /// </summary>
        public int Penalty { get; set; }

        /// <summary>
        /// The duration after which the punishment would expire.
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Whether the punishment is expired now.
        /// </summary>
        public bool IsExpired => Type != PunishmentType.Permanent && RecordTime + Duration <= DateTimeOffset.Now;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PunishmentType
    {
        /// <summary>
        /// The punishment needs further discussion, whose penalty won't be counted into total points.
        /// </summary>
        [Description("待定")]
        Pending,

        /// <summary>
        /// The punishment is in normal status.
        /// </summary>
        [Description("一般")]
        Normal,

        /// <summary>
        /// The punishment never expires.
        /// </summary>
        [Description("长期")]
        Permanent,

        /// <summary>
        /// The punishment would keep the user from participating in subsequent tournaments, which is also permanent.
        /// </summary>
        [Description("赛事处分")]
        Fatal,
    }
}
