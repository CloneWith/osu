// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// The chess pieces consumed together with this placement when it updates the owner of another piece.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly ConsumedPiece[] ConsumedPieces;

        public ChessPlacement(int? boardRow, int? boardColumn, TeamColour ownerTeam = TeamColour.Neutral, ChoiceType type = ChoiceType.Neutral,
                              int beatmapID = TournamentExtensions.RESERVED_BEATMAP_ID, ConsumedPiece[]? consumedPieces = null)
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
            ConsumedPieces = consumedPieces ?? [];
        }

        public override string ToString() => $"{(char)('A' + BoardColumn - 1)}{BoardRow}";

        /// <summary>
        /// Whether this placement corresponds to a Shiro placement.
        /// </summary>
        [JsonIgnore]
        public bool IsShiroPlacement => CurrentType == ChoiceType.Pick && BeatmapID == TournamentExtensions.RESERVED_BEATMAP_ID;

        /// <summary>
        /// Whether this placement represents an owner-update group (a win record that owns a non-empty <see cref="ConsumedPieces"/>).
        /// </summary>
        [JsonIgnore]
        public bool IsOwnerUpdate => CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin && ConsumedPieces.Length > 0;

        /// <summary>
        /// Whether this placement should be displayed in the match history.
        /// </summary>
        [JsonIgnore]
        public bool IsViewable => CurrentType is ChoiceType.Ban or ChoiceType.Pick or ChoiceType.RedWin or ChoiceType.BlueWin;

        /// <summary>
        /// The <see cref="HistoryType"/> this placement maps to for UI rendering. Derived from <see cref="CurrentType"/>,
        /// <see cref="BeatmapID"/> and <see cref="ConsumedPieces"/>.
        /// </summary>
        [JsonIgnore]
        public HistoryType DerivedHistoryType
        {
            get
            {
                if (CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin)
                    return ConsumedPieces.Length > 0 ? HistoryType.OwnerUpdate : HistoryType.Normal;

                return IsShiroPlacement ? HistoryType.ShiroPlacement : HistoryType.Normal;
            }
        }

        /// <summary>
        /// Look up the <see cref="RoundBeatmap"/> matching this placement's <see cref="BeatmapID"/> in
        /// <paramref name="beatmaps"/>, returning its <c>(Mod, ModIndex)</c> pair if found.
        /// </summary>
        public (string mod, string modIndex)? TryResolveMod(IReadOnlyList<RoundBeatmap>? beatmaps)
        {
            var match = beatmaps?.FirstOrDefault(b => b.ID == BeatmapID);
            return match is null ? null : (match.Mods, match.ModIndex);
        }

        /// <summary>
        /// Resolve the <c>(Mod, ModIndex)</c> pairs for each piece referenced by <see cref="ConsumedPieces"/>.
        /// Pieces that no longer resolve to a beatmap (e.g. round changed) are silently dropped — the
        /// displayed list reflects what is still meaningful.
        /// </summary>
        public (string mod, string modIndex)[] ResolveUsedPieces(IReadOnlyList<RoundBeatmap>? beatmaps)
        {
            if (beatmaps == null) return [];

            return ConsumedPieces
                   .Select(p => beatmaps.FirstOrDefault(b => b.ID == p.BeatmapID))
                   .OfType<RoundBeatmap>()
                   .Select(p => (p.Mods, p.ModIndex))
                   .ToArray();
        }

        /// <summary>
        /// Get an independent instance of <see cref="ChessPlacement"/> based on given new owner and type.
        /// </summary>
        /// <param name="newOwner">the new <see cref="TeamColour"/> of the team holding the chess.</param>
        /// <param name="newType">the new <see cref="ChoiceType"/> of the chess.</param>
        /// <returns>a <see cref="ChessPlacement"/> instance representing the updated chess.</returns>
        public ChessPlacement CreateUpdate(TeamColour? newOwner, ChoiceType? newType)
            => new ChessPlacement(BoardRow, BoardColumn, newOwner ?? OwnerTeam, newType ?? CurrentType, BeatmapID);

        /// <summary>
        /// Get an independent instance of <see cref="ChessPlacement"/> based on given new owner, type, and the set of pieces consumed
        /// together with this update. Used when this placement represents the "recolour" half of an atomic owner update.
        /// </summary>
        /// <param name="newOwner">the new <see cref="TeamColour"/> of the team holding the chess.</param>
        /// <param name="newType">the new <see cref="ChoiceType"/> of the chess.</param>
        /// <param name="consumedPieces">the chess pieces consumed together with this placement, or <c>null</c> for none.</param>
        /// <returns>a <see cref="ChessPlacement"/> instance representing the updated chess.</returns>
        public ChessPlacement CreateUpdate(TeamColour? newOwner, ChoiceType? newType, ConsumedPiece[]? consumedPieces)
            => new ChessPlacement(BoardRow, BoardColumn, newOwner ?? OwnerTeam, newType ?? CurrentType, BeatmapID, consumedPieces);

        /// <summary>
        /// Whether the board position (<paramref name="row"/>, <paramref name="column"/>) is consumed by a later
        /// owner-update placement in <paramref name="placements"/>.
        /// </summary>
        public static bool IsPositionConsumedBy(IEnumerable<ChessPlacement> placements, int row, int column)
            => placements.Any(p => p.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin
                                   && p.ConsumedPieces.Any(c => c.BoardRow == row && c.BoardColumn == column));

        /// <summary>
        /// Whether the beatmap with <paramref name="beatmapID"/> is consumed by a later owner-update placement in
        /// <paramref name="placements"/>.
        /// </summary>
        public static bool IsBeatmapConsumedBy(IEnumerable<ChessPlacement> placements, int beatmapID)
            => placements.Any(p => p.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin
                                   && p.ConsumedPieces.Any(c => c.BeatmapID == beatmapID));
    }

    /// <summary>
    /// Types of chess placement history used by UI rendering.
    /// </summary>
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum HistoryType
    {
        /// <summary>
        /// Normal ban, pick and win operations.
        /// </summary>
        Normal,

        /// <summary>
        /// A shiro was placed.
        /// </summary>
        ShiroPlacement,

        /// <summary>
        /// Chess pieces are consumed to update the owner of another chess piece.
        /// </summary>
        OwnerUpdate,
    }

    /// <summary>
    /// A lightweight reference to a chess piece consumed as part of an owner-update placement.
    /// </summary>
    /// <remarks>
    /// The piece is identified by its board position; <see cref="BeatmapID"/> is carried so that the consumed
    /// piece can be resolved back to a <see cref="RoundBeatmap"/> for history display purposes.
    /// </remarks>
    [Serializable]
    public class ConsumedPiece
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public readonly int BeatmapID;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public readonly int BoardRow;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public readonly int BoardColumn;

        [JsonConstructor]
        public ConsumedPiece(int beatmapID, int boardRow, int boardColumn)
        {
            BeatmapID = beatmapID;
            BoardRow = boardRow;
            BoardColumn = boardColumn;
        }

        /// <summary>
        /// Capture the consumed-piece reference from an existing placement (typically the last placement at the block being consumed).
        /// </summary>
        public ConsumedPiece(ChessPlacement placement)
            : this(placement.BeatmapID, placement.BoardRow, placement.BoardColumn)
        {
        }
    }
}
