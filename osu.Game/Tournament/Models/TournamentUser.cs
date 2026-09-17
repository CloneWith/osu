// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tournament.Localisation;
using osu.Game.Users;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A tournament player user, containing simple information about the player.
    /// </summary>
    [Serializable]
    public class TournamentUser : IUser
    {
        /// <summary>
        /// How long the fetched profile of a player is treated as "current".
        /// </summary>
        /// <remarks>
        /// <para>
        /// A player's pp and global rank drift over the course of an event, so the bracket refreshes them
        /// rather than keeping the first values it ever saw.
        /// </para>
        /// <para>
        /// The window is deliberately much longer than the session:
        /// within it the client never touches the network for a profile it already has, which is what
        /// keeps the client usable on a poor connection.
        /// </para>
        /// </remarks>
        public static readonly TimeSpan PROFILE_MAX_AGE = TimeSpan.FromHours(12);

        [JsonProperty(@"id")]
        public int OnlineID { get; set; }

        public string Username { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        /// <summary>
        /// The front-row tier the player was seeded into, or <see cref="DraftInfo.NO_TIER"/> when they are not
        /// a front-row player.
        /// </summary>
        public int Tier { get; set; }

        /// <summary>
        /// The player's country.
        /// </summary>
        [JsonProperty("country_code")]
        public CountryCode CountryCode { get; set; }

        /// <summary>
        /// The player's global rank, or null if not available.
        /// </summary>
        public int? Rank { get; set; }

        /// <summary>
        /// The player's performance points, or null if not available.
        /// </summary>
        public decimal? PP { get; set; }

        /// <summary>
        /// A URL to the player's profile cover.
        /// </summary>
        public string CoverUrl { get; set; } = string.Empty;

        /// <summary>
        /// When <see cref="Rank"/>, <see cref="PP"/> and <see cref="CoverUrl"/> were last fetched, or null if
        /// they have never been fetched.
        /// </summary>
        public DateTimeOffset? ProfileFetchedAt { get; set; }

        /// <summary>
        /// Whether the stored profile is missing or older than <see cref="PROFILE_MAX_AGE"/>.
        /// </summary>
        public bool ProfileIsStale => ProfileFetchedAt == null || DateTimeOffset.UtcNow - ProfileFetchedAt.Value > PROFILE_MAX_AGE;

        /// <summary>
        /// The player's rating information.
        /// </summary>
        public RatingInfo Ratings { get; set; } = new RatingInfo();

        public APIUser ToAPIUser()
        {
            var user = new APIUser
            {
                Id = OnlineID,
                Username = Username,
                CountryCode = CountryCode,
                CoverUrl = CoverUrl,
            };

            user.Statistics = new UserStatistics
            {
                User = user,
                GlobalRank = Rank,
                PP = PP
            };

            return user;
        }

        bool IUser.IsBot => false;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRole
    {
        /// <summary>
        /// Team member, the minimum role.
        /// </summary>
        [LocalisableDescription(typeof(BaseStrings), nameof(BaseStrings.TeamMember))]
        Member,

        /// <summary>
        /// Team members making decisions.
        /// </summary>
        [LocalisableDescription(typeof(BaseStrings), nameof(BaseStrings.TeamStrategist))]
        Strategist,

        /// <summary>
        /// Team leader.
        /// </summary>
        [LocalisableDescription(typeof(BaseStrings), nameof(BaseStrings.TeamLeader))]
        Leader,
    }
}
