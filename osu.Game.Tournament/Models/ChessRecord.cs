// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Represents an operation on chess attributes.
    /// </summary>
    [Serializable]
    public record ChessRecord
    {
        /// <summary>
        /// The <see cref="TeamColour"/> the operation is executed by (if exists).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public readonly TeamColour Colour = TeamColour.Neutral;

        /// <summary>
        /// The target <see cref="ChoiceType"/> of this operation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public readonly ChoiceType NewType = ChoiceType.Neutral;

        public ChessRecord(TeamColour colour, ChoiceType newType)
        {
            Colour = colour;
            NewType = newType;
        }
    }
}
