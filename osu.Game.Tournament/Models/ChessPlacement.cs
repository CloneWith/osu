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
        /// <summary>
        /// The team holding the chess right now, representing by a <see cref="TeamColour"/>.
        /// </summary>
        /// <remarks>This property isn't supposed to set directly. Use <see cref="Update"/> and <see cref="Undo"/> instead.</remarks>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour OwnerTeam { get; private set; }

        /// <summary>
        /// The current <see cref="ChoiceType"/> of the chess.
        /// </summary>
        /// <remarks>This property isn't supposed to set directly. Use <see cref="Update"/> and <see cref="Undo"/> instead.</remarks>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ChoiceType CurrentType { get; private set; }

        /// <summary>
        /// The ID of the beatmap the chess is associated with. Zero if it isn't associated with one.
        /// </summary>
        public readonly int BeatmapID;

        public readonly int BoardRow;
        public readonly int BoardColumn;

        /// <summary>
        /// A list of changes of this chess piece for easier backtracking.
        /// </summary>
        public readonly List<ChessRecord> Records = new List<ChessRecord>();

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

        /// <summary>
        /// Update the chess's status and record it to the <see cref="Records"/> list.
        /// </summary>
        /// <param name="newOwner">the new <see cref="TeamColour"/> of the team holding the chess.</param>
        /// <param name="newType">the new <see cref="ChoiceType"/> of the chess.</param>
        public void Update(TeamColour? newOwner, ChoiceType? newType)
        {
            if (newOwner == null && newType == null) return;

            if (newOwner != null)
                OwnerTeam = newOwner.Value;

            if (newType != null)
                CurrentType = newType.Value;

            Records.Add(new ChessRecord(OwnerTeam, CurrentType));
        }

        /// <summary>
        /// Restore the last status of the chess from the <see cref="Records"/> list.
        /// </summary>
        /// <returns>true if succeeded, false otherwise (no status to restore from)</returns>
        public bool Undo()
        {
            // Don't undo when only one record is found.
            if (Records.Count <= 1) return false;

            // Get and restore the last state.
            var lastRecord = Records[^2];

            OwnerTeam = lastRecord.Colour;
            CurrentType = lastRecord.NewType;

            // We don't need the latest record anymore.
            Records.RemoveAt(Records.Count - 1);
            return true;
        }
    }
}
