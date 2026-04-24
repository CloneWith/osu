// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Common and dedicate rating data for a <see cref="TournamentUser"/> or <see cref="TournamentTeam"/>.
    /// </summary>
    [Serializable]
    public class RatingInfo
    {
        /// <summary>
        /// The last update time of the rating information.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;

        /// <summary>
        /// The comprehensive rating value of the user / team.
        /// </summary>
        public float TotalRating { get; set; }

        public float NoMod { get; set; }
        public float HardRock { get; set; }
        public float DoubleTime { get; set; }
        public float Hidden { get; set; }

        public float Aim { get; set; }
        public float Tapping { get; set; }
        public float Technical { get; set; }
        public float LowApproach { get; set; }
        public float HighApproach { get; set; }
        public float HighSpeed { get; set; }
        public float Precision { get; set; }
    }
}
