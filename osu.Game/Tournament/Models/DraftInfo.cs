// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// The complete state of a single draft session.
    /// </summary>
    [Serializable]
    public class DraftInfo
    {
        public const int MIN_TEAM_COUNT = 1;
        public const int MAX_TEAM_COUNT = 8;
        public const int DEFAULT_TEAM_COUNT = 4;

        /// <summary>
        /// The tier a player carries when they are not part of the front row.
        /// </summary>
        public const int NO_TIER = 0;

        /// <summary>
        /// How many tiers the front row is split into. Each group takes one player per tier from the random
        /// phase, so this is also how many of a group's slots the random phase is responsible for.
        /// </summary>
        public const int FRONT_ROW_TIERS = 3;

        /// <summary>
        /// How many players each tier of the front row holds.
        /// </summary>
        public const int PLAYERS_PER_TIER = 8;

        /// <summary>
        /// How many players make up the front row, i.e. how many the random phase draws from. An event with
        /// fewer groups than <see cref="PLAYERS_PER_TIER"/> simply leaves the surplus front-row players
        /// undrawn; they take no part in the draft.
        /// </summary>
        public const int FRONT_ROW_SIZE = FRONT_ROW_TIERS * PLAYERS_PER_TIER;

        /// <summary>
        /// The smallest and largest member limit a group may be set to.
        /// </summary>
        /// <remarks>
        /// The lower bound is the size of the front row: a group always shows one slot per tier, so a smaller
        /// limit could not display what the random phase placed.
        /// </remarks>
        public const int MIN_MEMBERS_PER_TEAM = FRONT_ROW_TIERS;

        public const int MAX_MEMBERS_PER_TEAM = 16;

        public const int DEFAULT_MEMBERS_PER_TEAM = 4;

        /// <summary>
        /// The number of groups this draft is split into. Authoritative over the length of <see cref="Teams"/>.
        /// </summary>
        public BindableInt TeamCount { get; } = new BindableInt(DEFAULT_TEAM_COUNT)
        {
            MinValue = MIN_TEAM_COUNT,
            MaxValue = MAX_TEAM_COUNT,
        };

        /// <summary>
        /// The number of members a single group is limited to, front row included.
        /// </summary>
        public BindableInt MembersPerTeam { get; } = new BindableInt(DEFAULT_MEMBERS_PER_TEAM)
        {
            MinValue = MIN_MEMBERS_PER_TEAM,
            MaxValue = MAX_MEMBERS_PER_TEAM,
        };

        /// <summary>
        /// The draft groups, in drafting order.
        /// </summary>
        public BindableList<TournamentTeam> Teams { get; } = new BindableList<TournamentTeam>();

        /// <summary>
        /// The players which are still waiting to be placed — the front row which has not been drawn yet as well
        /// as the remaining players the draft picks from.
        /// </summary>
        /// <remarks>
        /// Both phases share the one pool and tell the two apart by <see cref="TournamentUser.Tier"/>, which keeps
        /// a player from ever being in two places at once and means the two screens can run in either order.
        /// </remarks>
        public BindableList<TournamentUser> Candidates { get; } = new BindableList<TournamentUser>();

        /// <summary>
        /// The players which have already been placed into a group, front row included.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<TournamentUser> AssignedPlayers => Teams.SelectMany(t => t.Players);

        /// <summary>
        /// Split the players into the front-row tiers, in the order the player list named them.
        /// </summary>
        /// <param name="playersInPlayerListOrder">
        /// Every player of the player list, in file order. Everything past <see cref="FRONT_ROW_SIZE"/> is left
        /// untiered, which is what puts it in the draft's pool rather than the random phase's.
        /// </param>
        public static void ClassifyFrontRow(IReadOnlyList<TournamentUser> playersInPlayerListOrder)
        {
            for (int i = 0; i < playersInPlayerListOrder.Count; i++)
                playersInPlayerListOrder[i].Tier = i < FRONT_ROW_SIZE ? i / PLAYERS_PER_TIER + 1 : NO_TIER;
        }

        /// <summary>
        /// The tier of the slot at <paramref name="index"/> of a group. The layout is fixed: the first
        /// <see cref="FRONT_ROW_TIERS"/> slots belong to the tiers and everything after them to a regular member.
        /// </summary>
        public static int TierForSlot(int index) => index < FRONT_ROW_TIERS ? index + 1 : NO_TIER;

        /// <summary>
        /// The player the random phase placed into the <paramref name="tier"/> slot of <paramref name="team"/>,
        /// or <c>null</c> while it is empty.
        /// </summary>
        /// <remarks>
        /// The tier is read from the player rather than from their position in the roster, because a group whose
        /// first tier has not been drawn yet still shows a slot for it.
        /// </remarks>
        public static TournamentUser? PlayerOfTier(TournamentTeam team, int tier)
            => tier == NO_TIER ? null : team.Players.FirstOrDefault(p => p.Tier == tier);

        /// <summary>
        /// The players filling the member positions of <paramref name="team"/>, in the order they were drafted.
        /// </summary>
        public static IEnumerable<TournamentUser> DraftedMembersOf(TournamentTeam team)
            => team.Players.Where(p => p.Tier == NO_TIER);

        /// <summary>
        /// Whether <paramref name="player"/> was placed by the random phase, which the draft cannot undo.
        /// </summary>
        public static bool IsFrontRow(TournamentUser player) => player.Tier != NO_TIER;

        /// <summary>
        /// How many of a group's slots are member positions under the current <see cref="MembersPerTeam"/>.
        /// </summary>
        public int MemberCapacity => Math.Max(0, MembersPerTeam.Value - FRONT_ROW_TIERS);

        /// <summary>
        /// Whether <paramref name="team"/> still has a free member position.
        /// </summary>
        public bool HasFreeMemberSlot(TournamentTeam team)
            => team.Players.Count < MembersPerTeam.Value && DraftedMembersOf(team).Count() < MemberCapacity;

        /// <summary>
        /// Whether the <paramref name="tier"/> slot of <paramref name="team"/> is still free.
        /// </summary>
        public bool HasFreeTierSlot(TournamentTeam team, int tier)
            => tier != NO_TIER && team.Players.Count < MembersPerTeam.Value && PlayerOfTier(team, tier) == null;

        /// <summary>
        /// Whether <paramref name="player"/> can be placed into <paramref name="team"/>. A front-row player goes
        /// to the slot of their own tier and an untiered one to the first free member position.
        /// </summary>
        public bool CanAssign(TournamentUser player, TournamentTeam team)
        {
            if (team.Players.Contains(player))
                return false;

            return player.Tier == NO_TIER ? HasFreeMemberSlot(team) : HasFreeTierSlot(team, player.Tier);
        }

        /// <summary>
        /// Move <paramref name="player"/> out of the pool and into <paramref name="team"/>.
        /// </summary>
        /// <returns>Whether the player was placed.</returns>
        /// <remarks>
        /// The group is updated before the pool, so that anything reacting to the pool losing its last player
        /// already sees that player as placed. Doing it the other way round leaves a gap in which the player
        /// is in neither collection.
        /// </remarks>
        public bool Assign(TournamentUser player, TournamentTeam team)
        {
            if (!CanAssign(player, team))
                return false;

            // The roster is kept in slot order — tier one to three, then the members — so a saved draft reads
            // the way the group is laid out, and the members a lower limit has to let go are always the tail.
            team.Players.Insert(insertIndexFor(team, player.Tier), player);

            Candidates.Remove(player);

            return true;
        }

        /// <summary>
        /// Send <paramref name="player"/> back to the pool.
        /// </summary>
        /// <returns>Whether the player was removed.</returns>
        /// <remarks>
        /// A front-row player is refused: they were placed by the random phase rather than by the draft, so
        /// handing them back is not the draft's decision to make. The players left behind keep the tiers they
        /// had, so a vacated slot stays empty rather than being taken over by the player behind it.
        /// </remarks>
        public bool Release(TournamentUser player, TournamentTeam team)
        {
            if (IsFrontRow(player))
                return false;

            if (!team.Players.Remove(player))
                return false;

            returnToPool(player);

            return true;
        }

        /// <summary>
        /// Empty every group, sending all players back to the pool, so that the selection can be run
        /// from the beginning again.
        /// </summary>
        public void ResetAssignments()
        {
            var released = Teams.SelectMany(t => t.Players).ToList();

            // Every roster is emptied before anything goes back to the pool, so no player is ever in two places
            // at once and the pool is only ever touched by one change.
            foreach (TournamentTeam team in Teams)
                team.Players.Clear();

            released.RemoveAll(p => Candidates.Contains(p));

            if (released.Count > 0)
                Candidates.AddRange(released);
        }

        /// <summary>
        /// Where a player taking the <paramref name="tier"/> slot of <paramref name="team"/> goes, so that the
        /// roster reads in slot order.
        /// </summary>
        private static int insertIndexFor(TournamentTeam team, int tier)
        {
            if (tier == NO_TIER)
                return team.Players.Count;

            return team.Players.Count(p => p.Tier != NO_TIER && p.Tier < tier);
        }

        /// <summary>
        /// Make the group setup match the settings, and release every player which no longer fits back into the pool.
        /// </summary>
        public void ReconcileTeams()
        {
            if (Teams.Count > TeamCount.Value)
            {
                var released = new List<TournamentUser>();

                for (int i = TeamCount.Value; i < Teams.Count; i++)
                    released.AddRange(Teams[i].Players);

                // A single bulk removal rather than one event per group, so consumers only react once.
                Teams.RemoveRange(TeamCount.Value, Teams.Count - TeamCount.Value);

                foreach (TournamentUser player in released)
                    returnToPool(player);
            }
            else if (Teams.Count < TeamCount.Value)
            {
                var added = new List<TournamentTeam>();

                for (int i = Teams.Count; i < TeamCount.Value; i++)
                    added.Add(CreateGroup(i));

                Teams.AddRange(added);
            }

            int capacity = MemberCapacity;

            foreach (TournamentTeam team in Teams)
            {
                // A group has room for one player per tier and as many members as the limit leaves room for.
                // Anything past that — a duplicated tier surviving in a hand-edited file included — has no slot
                // to be shown in, so it goes back to the pool.
                var losingSlot = Enumerable.Range(1, FRONT_ROW_TIERS)
                                           .SelectMany(tier => team.Players.Where(p => p.Tier == tier).Skip(1))
                                           .Concat(DraftedMembersOf(team).Skip(capacity))
                                           .ToList();

                // The players which stay keep their tiers, so lowering the limit leaves a slot empty rather
                // than promoting whoever comes after it.
                foreach (TournamentUser player in losingSlot)
                {
                    team.Players.Remove(player);
                    returnToPool(player);
                }
            }
        }

        /// <summary>
        /// Create an empty draft group, named after its position.
        /// </summary>
        /// <remarks>
        /// The positional name is only a placeholder: a group standing for one of the event's teams takes that
        /// team's name instead, which the session applies as the group is created and whenever the draft is
        /// read back.
        /// </remarks>
        public static TournamentTeam CreateGroup(int index)
        {
            string name = GroupNameForPosition(index);

            return new TournamentTeam
            {
                FullName = { Value = name },
                Acronym = { Value = name },
            };
        }

        /// <summary>
        /// The name a group falls back to while the bracket has no team at its position: A, B, C, ...
        /// </summary>
        public static string GroupNameForPosition(int index) => ((char)('A' + index)).ToString();

        /// <summary>
        /// Put <paramref name="player"/> back in the pool.
        /// </summary>
        private void returnToPool(TournamentUser player)
        {
            if (!Candidates.Contains(player))
                Candidates.Add(player);
        }
    }
}
