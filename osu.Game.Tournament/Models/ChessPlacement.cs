// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Logging;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Represents a placement of a chess piece.
    /// </summary>
    [Serializable]
    public class ChessPlacement
    {
        /// <summary>
        /// The team holding the chess right now, representing by a <see cref="TeamColour"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public readonly TeamColour OwnerTeam;

        /// <summary>
        /// The current <see cref="ChoiceType"/> of the chess.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public readonly ChoiceType CurrentType;

        /// <summary>
        /// The ID of the beatmap the chess is associated with. Zero if it isn't associated with one.
        /// </summary>
        public readonly int BeatmapID;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public readonly int BoardRow;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public readonly int BoardColumn;

        public ChessPlacement(int? boardRow, int? boardColumn, TeamColour ownerTeam = TeamColour.Neutral, ChoiceType type = ChoiceType.Neutral, int beatmapID = 0)
        {
            BeatmapID = beatmapID;
            OwnerTeam = ownerTeam;
            CurrentType = type;

            if (boardRow <= -1 || boardRow > 4 || boardColumn <= -1 || boardColumn > 4)
            {
                Logger.Log($"The position of the chess ({boardRow}, {boardColumn}) is out of range. Please check the bracket file.",
                    level: LogLevel.Important);
            }

            BoardRow = boardRow ?? -1;
            BoardColumn = boardColumn ?? -1;
        }

        /// <summary>
        /// Get an independent instance of <see cref="ChessPlacement"/> based on given new owner and type.
        /// </summary>
        /// <param name="newOwner">the new <see cref="TeamColour"/> of the team holding the chess.</param>
        /// <param name="newType">the new <see cref="ChoiceType"/> of the chess.</param>
        /// <returns>a <see cref="ChessPlacement"/> instance representing the updated chess.</returns>
        public ChessPlacement CreateUpdate(TeamColour? newOwner, ChoiceType? newType)
            => new ChessPlacement(BoardRow, BoardColumn, newOwner ?? OwnerTeam, newType ?? CurrentType, BeatmapID);
    }
}
