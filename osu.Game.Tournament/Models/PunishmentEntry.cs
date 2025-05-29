// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
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
        public Bindable<DateTimeOffset> RecordTime = new Bindable<DateTimeOffset>(DateTimeOffset.Now);

        /// <summary>
        /// The online ID of the user receiving the punishment.
        /// </summary>
        public BindableInt UserID = new BindableInt();

        /// <summary>
        /// The type of the punishment.
        /// </summary>
        public Bindable<PunishmentType> Type = new Bindable<PunishmentType>(PunishmentType.Normal);

        /// <summary>
        /// The reason of the punishment. Just for archiving purposes.
        /// </summary>
        public Bindable<string> Reason = new Bindable<string>();

        /// <summary>
        /// Amount of warning points one gets for the punishment.
        /// </summary>
        public BindableInt Penalty = new BindableInt(1)
        {
            MinValue = 1,
            MaxValue = 10,
        };

        /// <summary>
        /// The time the punishment would expire.
        /// </summary>
        public Bindable<DateTimeOffset> ExpireTime = new Bindable<DateTimeOffset>(DateTimeOffset.Now.AddDays(7));

        /// <summary>
        /// Whether the punishment is expired now.
        /// </summary>
        [JsonIgnore]
        public bool IsExpired => Type.Value is not (PunishmentType.Permanent or PunishmentType.Fatal)
                                 && ExpireTime.Value <= DateTimeOffset.Now;
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
