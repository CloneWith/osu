// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// The state of a draft session, shared by the two screens which run it.
    /// </summary>
    public sealed class DraftSession
    {
        /// <summary>
        /// The file the pool player IDs are read from. One user ID per line, in seeding order.
        /// </summary>
        public const string PLAYER_LIST_FILENAME = @"draft_players.txt";

        /// <summary>
        /// The file the draft state is serialised to.
        /// </summary>
        public const string DRAFT_FILENAME = @"draft.json";

        /// <summary>
        /// The settings the draft is persisted with.
        /// </summary>
        public static JsonSerializerSettings SerialisationSettings { get; } = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        private readonly Storage storage;

        /// <summary>
        /// Whether the persisted draft has been read into <see cref="Draft"/> yet.
        /// </summary>
        /// <remarks>
        /// Both screens ask for the draft as they load, and every screen is constructed eagerly at startup, so
        /// without this the second one to ask would read the file over the state the first one had just set up.
        /// </remarks>
        private bool loaded;

        public DraftSession(Storage storage)
        {
            this.storage = storage;
        }

        /// <summary>
        /// The draft itself, mutated in place.
        /// </summary>
        public DraftInfo Draft { get; } = new DraftInfo();

        #region Reading and writing

        /// <summary>
        /// Read the persisted draft into <see cref="Draft"/> unless that has already been done.
        /// </summary>
        public void EnsureLoaded(LadderInfo ladder)
        {
            if (loaded)
                return;

            Reload(ladder);
        }

        /// <summary>
        /// Read the persisted draft into <see cref="Draft"/> again, discarding whatever is in memory.
        /// </summary>
        public void Reload(LadderInfo ladder)
        {
            loaded = true;

            // Wiping the settings is safe because the groups are reconciled only once the file (or the bracket)
            // has had its say — a group count materialized from the defaults would otherwise pre-empt it.
            Draft.Teams.Clear();
            Draft.Candidates.Clear();
            Draft.TeamCount.Value = DraftInfo.DEFAULT_TEAM_COUNT;
            Draft.MembersPerTeam.Value = DraftInfo.DEFAULT_MEMBERS_PER_TEAM;

            readDraftFile();

            if (Draft.Teams.Count == 0)
                seedGroups(ladder);

            Draft.ReconcileTeams();

            // Last, so that both a seeded draft and one read back from disk end up named after the teams the
            // event actually has — a file written before the bracket was set up carries nothing but the
            // positional fallbacks.
            SyncGroupNames(ladder);
        }

        /// <summary>
        /// Persist the draft. Called as it changes as well as on demand, so an interrupted session survives.
        /// </summary>
        public void Save()
        {
            try
            {
                string serialised = JsonConvert.SerializeObject(Draft, SerialisationSettings);

                using Stream stream = storage.CreateFileSafely(DRAFT_FILENAME);
                using var sw = new StreamWriter(stream);
                sw.Write(serialised);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $@"Failed to write {DRAFT_FILENAME}.");
            }
        }

        private void readDraftFile()
        {
            if (!storage.Exists(DRAFT_FILENAME))
                return;

            try
            {
                DraftInfo? loadedDraft;

                using (Stream stream = storage.GetStream(DRAFT_FILENAME, FileAccess.Read, FileMode.Open))
                using (var sr = new StreamReader(stream))
                    loadedDraft = JsonConvert.DeserializeObject<DraftInfo>(sr.ReadToEnd());

                if (loadedDraft == null)
                    return;

                // Copied across (rather than the instance being replaced) and in bulk, so consumers only see
                // one change per collection.
                Draft.TeamCount.Value = loadedDraft.TeamCount.Value;
                Draft.MembersPerTeam.Value = loadedDraft.MembersPerTeam.Value;
                Draft.Teams.AddRange(loadedDraft.Teams);

                if (loadedDraft.Candidates.Count > 0)
                    Draft.Candidates.AddRange(loadedDraft.Candidates);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $@"Failed to read {DRAFT_FILENAME}, starting a new draft.");
            }
        }

        /// <summary>
        /// Create one empty group per bracket team, so that the boxes cover the event being run.
        /// </summary>
        /// <remarks>
        /// Only the number of groups is decided here; what they are called comes from
        /// <see cref="SyncGroupNames"/>, which also covers the groups created later on.
        /// </remarks>
        private void seedGroups(LadderInfo ladder)
        {
            int bracketTeams = ladder.Teams.Count;

            int count = bracketTeams > 0
                ? Math.Clamp(bracketTeams, DraftInfo.MIN_TEAM_COUNT, DraftInfo.MAX_TEAM_COUNT)
                : DraftInfo.DEFAULT_TEAM_COUNT;

            Draft.TeamCount.Value = count;

            var seeded = new List<TournamentTeam>();

            for (int i = 0; i < count; i++)
                seeded.Add(DraftInfo.CreateGroup(i));

            // One bulk insertion, which the group listing picks up in a single change.
            Draft.Teams.AddRange(seeded);
        }

        /// <summary>
        /// Name every group after the bracket team it stands for, falling back to its position (A, B, C, ...)
        /// where the bracket has no team for it.
        /// </summary>
        public void SyncGroupNames(LadderInfo ladder)
        {
            for (int i = 0; i < Draft.Teams.Count; i++)
            {
                TournamentTeam group = Draft.Teams[i];
                TournamentTeam? source = i < ladder.Teams.Count ? ladder.Teams[i] : null;

                string fallback = DraftInfo.GroupNameForPosition(i);

                group.FullName.Value = string.IsNullOrEmpty(source?.FullName.Value) ? fallback : source.FullName.Value;
                group.Acronym.Value = string.IsNullOrEmpty(source?.Acronym.Value) ? fallback : source.Acronym.Value;
                group.FlagName.Value = source?.FlagName.Value ?? string.Empty;
            }
        }

        #endregion

        #region The player list

        /// <summary>
        /// Why reading the player list did not produce a pool.
        /// </summary>
        public enum PlayerListReadResult
        {
            Success,
            Missing,
            Unreadable,
            Empty,
        }

        /// <summary>
        /// Read the pool player IDs from <see cref="PLAYER_LIST_FILENAME"/>. The file is only ever used as the
        /// input for the pool — once read, everything is driven by the draft model.
        /// </summary>
        public PlayerListReadResult ReadPlayerIds(out List<int> ids)
        {
            ids = new List<int>();

            if (!storage.Exists(PLAYER_LIST_FILENAME))
                return PlayerListReadResult.Missing;

            try
            {
                using Stream stream = storage.GetStream(PLAYER_LIST_FILENAME, FileAccess.Read, FileMode.Open);
                using var sr = new StreamReader(stream);

                while (sr.ReadLine() is string line)
                {
                    line = line.Trim();

                    if (line.Length > 0 && int.TryParse(line, out int id) && id > 0 && !ids.Contains(id))
                        ids.Add(id);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $@"Failed to read {PLAYER_LIST_FILENAME}.");
                return PlayerListReadResult.Unreadable;
            }

            return ids.Count == 0 ? PlayerListReadResult.Empty : PlayerListReadResult.Success;
        }

        /// <summary>
        /// Replace the pool with the players the player list names, and hand them back in the order they were
        /// listed.
        /// </summary>
        /// <param name="ids">The IDs <see cref="ReadPlayerIds"/> produced.</param>
        public IReadOnlyList<TournamentUser> FillPool(IReadOnlyList<int> ids)
        {
            var known = new Dictionary<int, TournamentUser>();

            foreach (TournamentUser player in Draft.AssignedPlayers.Concat(Draft.Candidates))
                known[player.OnlineID] = player;

            var pool = ids.Select(id => known.TryGetValue(id, out TournamentUser? player)
                ? player
                : new TournamentUser { OnlineID = id }).ToList();

            DraftInfo.ClassifyFrontRow(pool);

            // A player which has already been placed is kept where they are rather than being handed back to the
            // pool: re-reading the list must not undo a draw. The rest go in with their tier, which is what the
            // two screens tell the front row and the remaining players apart by.
            var placed = Draft.AssignedPlayers.Select(p => p.OnlineID).ToHashSet();

            pool.RemoveAll(p => placed.Contains(p.OnlineID));

            Draft.Candidates.Clear();

            // One bulk insertion; the pool listing adds one panel per player.
            if (pool.Count > 0)
                Draft.Candidates.AddRange(pool);

            return pool;
        }

        #endregion

        #region The bracket

        /// <summary>
        /// Commit every group's roster to the bracket team it stands for, creating bracket teams for groups
        /// which have none.
        /// </summary>
        /// <returns>How many players were written.</returns>
        public int WriteToBracket(LadderInfo ladder)
        {
            int written = 0;

            for (int i = 0; i < Draft.Teams.Count; i++)
            {
                TournamentTeam source = Draft.Teams[i];
                TournamentTeam? target = findBracketTeam(ladder, source, i);

                if (target == null)
                {
                    target = new TournamentTeam
                    {
                        FullName = { Value = source.FullName.Value },
                        Acronym = { Value = source.Acronym.Value },
                        FlagName = { Value = source.FlagName.Value },
                    };

                    ladder.Teams.Add(target);
                }

                // Replacing a roster in bulk keeps this to a single change for anything watching the bracket
                // teams, and makes sure the previous roster cannot survive through a stale reference.
                target.Players.Clear();

                if (source.Players.Count > 0)
                    target.Players.AddRange(source.Players);

                written += source.Players.Count;
            }

            return written;
        }

        /// <summary>
        /// Match a draft group back to a bracket team, preferring the acronym (draft groups are seeded with it)
        /// and falling back to the group's position.
        /// </summary>
        private static TournamentTeam? findBracketTeam(LadderInfo ladder, TournamentTeam source, int index)
        {
            if (!string.IsNullOrEmpty(source.Acronym.Value))
            {
                var byAcronym = ladder.Teams.FirstOrDefault(t => string.Equals(t.Acronym.Value, source.Acronym.Value, StringComparison.OrdinalIgnoreCase));

                if (byAcronym != null)
                    return byAcronym;
            }

            if (!string.IsNullOrEmpty(source.FullName.Value))
            {
                var byName = ladder.Teams.FirstOrDefault(t => string.Equals(t.FullName.Value, source.FullName.Value, StringComparison.OrdinalIgnoreCase));

                if (byName != null)
                    return byName;
            }

            return index < ladder.Teams.Count ? ladder.Teams[index] : null;
        }

        #endregion
    }
}
