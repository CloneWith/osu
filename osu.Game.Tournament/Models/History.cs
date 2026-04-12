// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Represents a single operation on the board in a simpler and more direct form.
    /// </summary>
    public record History(
        HistoryType Type,
        TeamColour Team,
        ChoiceType OriginalType,
        string? Mod,
        string? ModIndex,
        int Row,
        int Column,
        (string mod, string modIndex)[] UsedPieces);

    /// <summary>
    /// Types of chess placement history used in <see cref="History"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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

    public static class HistoryExtensions
    {
        /// <summary>
        /// Convert <see cref="ChessPlacement"/> records to a list of <see cref="History"/>s using <see cref="RoundBeatmap"/>s.
        /// </summary>
        /// <param name="source">The chess placements of a match.</param>
        /// <param name="beatmaps">The beatmaps used in the <see cref="TournamentRound"/> the match belongs to.</param>
        /// <returns>A list of converted <see cref="History"/>s.</returns>
        public static List<History> Convert(List<ChessPlacement> source, List<RoundBeatmap>? beatmaps)
        {
            if (beatmaps == null)
                return [];

            List<History> result = new List<History>();

            for (int i = 0; i < source.Count; i++)
            {
                var entry = source[i];
                var match = beatmaps.FirstOrDefault(b => b.ID == entry.BeatmapID);
                bool isShiroPlacement = entry.CurrentType is ChoiceType.Pick && entry.BeatmapID == -1;

                switch (entry.CurrentType)
                {
                    case ChoiceType.Ban or ChoiceType.Pick:
                        result.Add(new History(isShiroPlacement ? HistoryType.ShiroPlacement : HistoryType.Normal,
                            entry.OwnerTeam, entry.CurrentType, match?.Mods, match?.ModIndex, entry.BoardRow, entry.BoardColumn, []));
                        break;

                    case ChoiceType.RedWin or ChoiceType.BlueWin:
                        var sequential = source.Skip(i + 1).TakeWhile(p => p.CurrentType is ChoiceType.Consumed).ToList();
                        var consumedMaps = sequential.Select(p => beatmaps.FirstOrDefault(b => b.ID == p.BeatmapID))
                                                     .OfType<RoundBeatmap>().ToList();

                        // Mark as owner updates if we have consumed records before next record
                        result.Add(new History(sequential.Any() ? HistoryType.OwnerUpdate : HistoryType.Normal,
                            entry.OwnerTeam, entry.CurrentType, match?.Mods, match?.ModIndex, entry.BoardRow, entry.BoardColumn,
                            consumedMaps.Select(p => (p.Mods, p.ModIndex)).ToArray()));

                        break;
                }
            }

            return result;
        }
    }
}
