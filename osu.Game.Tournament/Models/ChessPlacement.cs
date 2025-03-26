// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Represents a placement of a chess piece.
    /// </summary>
    [Serializable]
    public class ChessPlacement
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour OwnerTeam = TeamColour.Neutral;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ChoiceType CurrentType = ChoiceType.Neutral;

        /// <summary>
        /// The ID of the beatmap the chess is associated with. Zero if it isn't associated with one.
        /// </summary>
        public readonly int BeatmapID;

        public readonly int BoardRow;
        public readonly int BoardColumn;

        /// <summary>
        /// A list of changes of this chess piece for easier backtracking.
        /// </summary>
        public List<ChessRecord> Records = new List<ChessRecord>();

        public ChessPlacement(int boardRow, int boardColumn, TeamColour ownerTeam = TeamColour.Neutral, ChoiceType type = ChoiceType.Neutral, int beatmapID = 0)
        {
            BeatmapID = beatmapID;
            OwnerTeam = ownerTeam;
            CurrentType = type;

            Debug.Assert(boardRow > 0 && boardRow <= 4, "Board row out of range.");
            Debug.Assert(boardColumn > 0 && boardColumn <= 4, "Board column out of range.");

            BoardRow = boardRow;
            BoardColumn = boardColumn;

            Records.Add(new ChessRecord(OwnerTeam, CurrentType));
        }
    }
}
