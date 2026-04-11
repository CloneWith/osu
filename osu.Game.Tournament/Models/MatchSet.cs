// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    public class MatchSet
    {
        // TODO: set scores should really be stored in MatchSet instead of in TournamentMatch

        public Bindable<long> Map1Id = new Bindable<long>();
        public Bindable<long> Map2Id = new Bindable<long>();
        public Bindable<long> Map3Id = new Bindable<long>();
        public readonly bool IsTiebreaker;

        public MatchSet(bool isTiebreaker)
        {
            IsTiebreaker = isTiebreaker;
        }

        public static MatchSet? FindSetByMapId(TournamentMatch match, long mapId)
        {
            return match.Sets.FirstOrDefault(s => s.Map1Id.Value == mapId || s.Map2Id.Value == mapId || s.Map3Id.Value == mapId);
        }

        public Tuple<long, long>? GetSetScores(TournamentMatch currentMatch)
        {
            // lookup roundbeatmap
            var map1RoundBeatmap = currentMatch.Round.Value?.Beatmaps.FirstOrDefault(poolMap => poolMap.ID == Map1Id.Value);
            var map2RoundBeatmap = currentMatch.Round.Value?.Beatmaps.FirstOrDefault(poolMap => poolMap.ID == Map2Id.Value);
            var map3RoundBeatmap = currentMatch.Round.Value?.Beatmaps.FirstOrDefault(poolMap => poolMap.ID == Map3Id.Value);

            if (map1RoundBeatmap == null && map2RoundBeatmap == null)
                return null;

            // get scores from both maps
            long scoreRed = 0;
            long scoreBlue = 0;

            if (currentMatch.MapScores.TryGetValue(map1RoundBeatmap?.SlotName ?? "INVALID_SLOT", out var map1Score))
            {
                scoreRed += map1Score.Item1;
                scoreBlue += map1Score.Item2;
            }

            if (currentMatch.MapScores.TryGetValue(map2RoundBeatmap?.SlotName ?? "INVALID_SLOT", out var map2Score))
            {
                scoreRed += map2Score.Item1;
                scoreBlue += map2Score.Item2;
            }

            if (currentMatch.MapScores.TryGetValue(map3RoundBeatmap?.SlotName ?? "INVALID_SLOT", out var map3Score))
            {
                scoreRed += map3Score.Item1;
                scoreBlue += map3Score.Item2;
            }

            return new Tuple<long, long>(scoreRed, scoreBlue);
        }

        /// <summary>
        /// Helper to get the set scores for both players.
        /// </summary>
        /// <param name="currentMatch"></param>
        /// <param name="mapId"></param>
        /// <returns></returns>
        public static Tuple<long, long>? GetSetScores(TournamentMatch currentMatch, long mapId)
        {
            // get the current set
            var currentSet = FindSetByMapId(currentMatch, mapId);

            return currentSet?.GetSetScores(currentMatch);
        }
    }
}
