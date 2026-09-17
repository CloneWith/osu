// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class DraftInfoSerialisationTest
    {
        /// <summary>
        /// The front row is decided by the order the player list names its players in: the first
        /// <see cref="DraftInfo.FRONT_ROW_SIZE"/> of them are split into equal tiers, and everybody after them is
        /// left untiered, which is what puts them in the draft's hands rather than in the random phase's.
        /// </summary>
        [Test]
        public void TestClassifyFrontRow()
        {
            var players = Enumerable.Range(0, DraftInfo.FRONT_ROW_SIZE + 3)
                                    .Select(i => new TournamentUser { OnlineID = 1000 + i })
                                    .ToList();

            DraftInfo.ClassifyFrontRow(players);

            for (int tier = 1; tier <= DraftInfo.FRONT_ROW_TIERS; tier++)
            {
                Assert.That(players.Take(DraftInfo.FRONT_ROW_SIZE).Count(p => p.Tier == tier), Is.EqualTo(DraftInfo.PLAYERS_PER_TIER),
                    $"tier {tier} should hold a full tier");
            }

            Assert.That(players.Skip(DraftInfo.FRONT_ROW_SIZE).All(p => p.Tier == DraftInfo.NO_TIER), Is.True,
                "every player past the front row is the draft's");

            // The tiers are contiguous blocks of list order, so the first eight are the top tier and the ninth
            // starts the second.
            Assert.That(players[0].Tier, Is.EqualTo(1));
            Assert.That(players[DraftInfo.PLAYERS_PER_TIER - 1].Tier, Is.EqualTo(1));
            Assert.That(players[DraftInfo.PLAYERS_PER_TIER].Tier, Is.EqualTo(2));

            // Re-classifying is what re-reading the player list does, and the tiers are derived from the order
            // alone, so the same list gives the same answer rather than drifting.
            DraftInfo.ClassifyFrontRow(players);

            Assert.That(players[0].Tier, Is.EqualTo(1));
            Assert.That(players[DraftInfo.PLAYERS_PER_TIER].Tier, Is.EqualTo(2));
            Assert.That(players[DraftInfo.FRONT_ROW_SIZE].Tier, Is.EqualTo(DraftInfo.NO_TIER));
        }

        /// <summary>
        /// A group's slots are fixed: the first ones belong to the tiers and the rest to the members, which is
        /// what lets a tier which has not been drawn yet be shown as a position rather than as a gap.
        /// </summary>
        [Test]
        public void TestTierForSlot()
        {
            Assert.That(DraftInfo.TierForSlot(0), Is.EqualTo(1));
            Assert.That(DraftInfo.TierForSlot(DraftInfo.FRONT_ROW_TIERS - 1), Is.EqualTo(DraftInfo.FRONT_ROW_TIERS));
            Assert.That(DraftInfo.TierForSlot(DraftInfo.FRONT_ROW_TIERS), Is.EqualTo(DraftInfo.NO_TIER));
            Assert.That(DraftInfo.TierForSlot(99), Is.EqualTo(DraftInfo.NO_TIER));
        }

        /// <summary>
        /// A group takes one player per tier, so a second player of the same tier is refused and stays in the
        /// pool, requiring the draw to be able to find them a different group.
        /// </summary>
        [Test]
        public void TestTierSlotsTakeOnePlayerEach()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 1 },
            };

            draft.ReconcileTeams();

            var first = new TournamentUser { OnlineID = 1, Tier = 1 };
            var second = new TournamentUser { OnlineID = 2, Tier = 1 };

            draft.Candidates.AddRange(new[] { first, second });

            Assert.That(draft.Assign(first, draft.Teams[0]), Is.True);
            Assert.That(draft.Assign(second, draft.Teams[0]), Is.False, "a group takes one player per tier");
            Assert.That(draft.Candidates, Is.EquivalentTo(new[] { second }), "the refused player stays in the pool");
            Assert.That(DraftInfo.PlayerOfTier(draft.Teams[0], 1), Is.SameAs(first));
        }

        /// <summary>
        /// A front-row player was placed by the random phase, so handing them back is not the draft's to decide,
        /// not even to free their position up for somebody else.
        /// </summary>
        [Test]
        public void TestFrontRowCannotBeReleased()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 1 },
                MembersPerTeam = { Value = 4 },
            };

            draft.ReconcileTeams();

            var frontRow = new TournamentUser { OnlineID = 1, Tier = 1 };
            var member = new TournamentUser { OnlineID = 2 };

            draft.Candidates.AddRange(new[] { frontRow, member });

            Assert.That(draft.Assign(frontRow, draft.Teams[0]), Is.True);
            Assert.That(draft.Assign(member, draft.Teams[0]), Is.True);

            Assert.That(draft.Release(frontRow, draft.Teams[0]), Is.False, "the draft cannot undo a draw");
            Assert.That(draft.Teams[0].Players.ToArray(), Is.EquivalentTo(new[] { frontRow, member }));

            Assert.That(draft.Release(member, draft.Teams[0]), Is.True);
            Assert.That(draft.Candidates, Does.Contain(member));
            Assert.That(member.Tier, Is.EqualTo(DraftInfo.NO_TIER), "a player sent back has no tier");
        }

        /// <summary>
        /// The member limit covers the whole roster, front row included, so a group holds whatever the limit
        /// leaves past the tier positions.
        /// </summary>
        [Test]
        public void TestMemberCapacityLeavesTheFrontRowRoom()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 1 },
                MembersPerTeam = { Value = 5 },
            };

            draft.ReconcileTeams();

            Assert.That(draft.MemberCapacity, Is.EqualTo(5 - DraftInfo.FRONT_ROW_TIERS));

            var players = Enumerable.Range(0, 3).Select(i => new TournamentUser { OnlineID = 100 + i }).ToList();

            draft.Candidates.AddRange(players);

            Assert.That(draft.Assign(players[0], draft.Teams[0]), Is.True);
            Assert.That(draft.Assign(players[1], draft.Teams[0]), Is.True);
            Assert.That(draft.Assign(players[2], draft.Teams[0]), Is.False, "a group at its limit should refuse another member");

            Assert.That(draft.Teams[0].Players.Count, Is.EqualTo(2));
            Assert.That(draft.Candidates, Is.EquivalentTo(new[] { players[2] }), "the refused player should stay in the pool");
        }

        /// <summary>
        /// Lowering the limit hands the members which no longer fit back, and the front row keeps its positions,
        /// so a group never shows a tier player in a member position.
        /// </summary>
        [Test]
        public void TestLoweringMemberLimitReleasesTheTail()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 1 },
            };

            draft.ReconcileTeams();

            var frontRow = new TournamentUser { OnlineID = 1, Tier = 2 };
            var member = new TournamentUser { OnlineID = 2 };

            draft.Candidates.AddRange(new[] { frontRow, member });

            Assert.That(draft.Assign(frontRow, draft.Teams[0]), Is.True);
            Assert.That(draft.Assign(member, draft.Teams[0]), Is.True, "a limit of four leaves one member position");
            Assert.That(draft.Teams[0].Players.ToArray(), Is.EqualTo(new[] { frontRow, member }), "the roster reads in slot order");

            // The smallest limit is the size of the front row, so there is no member position left at all.
            draft.MembersPerTeam.Value = DraftInfo.MIN_MEMBERS_PER_TEAM;
            draft.ReconcileTeams();

            Assert.That(draft.Teams[0].Players.ToArray(), Is.EqualTo(new[] { frontRow }), "the front row keeps its position");
            Assert.That(draft.Candidates, Is.EquivalentTo(new[] { member }));
            Assert.That(member.Tier, Is.EqualTo(DraftInfo.NO_TIER));
        }

        /// <summary>
        /// The pool is the only place a player can be, so reconciling a smaller group count has to hand the
        /// members of the dropped groups back rather than losing them.
        /// </summary>
        [Test]
        public void TestReconcileReleasesDroppedGroups()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 3 },
            };

            draft.ReconcileTeams();

            var frontRow = new TournamentUser { OnlineID = 1, Tier = 1 };
            var member = new TournamentUser { OnlineID = 2 };

            draft.Candidates.AddRange(new[] { frontRow, member });

            Assert.That(draft.Assign(frontRow, draft.Teams[2]), Is.True);
            Assert.That(draft.Assign(member, draft.Teams[2]), Is.True);

            draft.TeamCount.Value = 1;
            draft.ReconcileTeams();

            Assert.That(draft.Teams.Count, Is.EqualTo(1));
            Assert.That(draft.Candidates, Is.EquivalentTo(new[] { frontRow, member }));
            Assert.That(frontRow.Tier, Is.EqualTo(1), "a front-row player handed back has to stay drawable");
            Assert.That(member.Tier, Is.EqualTo(DraftInfo.NO_TIER));
        }

        /// <summary>
        /// A group has one position per tier, so a second player of the same tier (which only a hand-edited
        /// file can produce) has nowhere to be shown and goes back to the pool.
        /// </summary>
        [Test]
        public void TestDuplicateTiersGoBackToThePool()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 1 },
            };

            draft.ReconcileTeams();

            var first = new TournamentUser { OnlineID = 1, Tier = 1 };
            var duplicate = new TournamentUser { OnlineID = 2, Tier = 1 };
            var second = new TournamentUser { OnlineID = 3, Tier = 2 };

            // Straight into the roster, exactly as a loaded file would leave it.
            draft.Teams[0].Players.AddRange(new[] { first, duplicate, second });

            draft.ReconcileTeams();

            Assert.That(draft.Teams[0].Players.Select(p => p.Tier), Is.EqualTo(new[] { 1, 2 }), "one player per tier survives");
            Assert.That(draft.Candidates, Is.EquivalentTo(new[] { duplicate }));
        }

        /// <summary>
        /// The role is the operator's annotation in the team editor rather than something the draft decides, so
        /// drafting a player must not touch it.
        /// </summary>
        [Test]
        public void TestDraftLeavesTheRoleAlone()
        {
            var draft = new DraftInfo
            {
                TeamCount = { Value = 1 },
            };

            draft.ReconcileTeams();

            var player = new TournamentUser { OnlineID = 1, Role = UserRole.Strategist };

            draft.Candidates.Add(player);

            Assert.That(draft.Assign(player, draft.Teams[0]), Is.True);
            Assert.That(player.Role, Is.EqualTo(UserRole.Strategist));

            Assert.That(draft.Release(player, draft.Teams[0]), Is.True);
            Assert.That(player.Role, Is.EqualTo(UserRole.Strategist), "not even handing them back rewrites it");
        }
    }
}
