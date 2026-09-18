// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Input;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Draft;
using osu.Game.Tournament.Screens.Draft.Components;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneDraftScreen : TournamentScreenTestScene
    {
        /// <summary>
        /// How many players the player list names past the front row, i.e. everybody this screen can draft.
        /// </summary>
        private const int pool_players = 6;

        private Storage storage = null!;

        [Resolved]
        private DraftSession session { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            this.storage = storage;

            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DraftScreen()
            });
        }

        [Test]
        public void TestDraftFlow()
        {
            AddStep("write the player list", writePlayerList);

            // The persisted draft is shared with the other tests in this fixture, so a fresh one is started.
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("only the players past the front row are offered", () => pool.Length == pool_players);

            // The groups are seeded from the bracket rather than from the settings, which is only the case as
            // long as nothing materialises them from the settings while the draft is being restored.
            AddAssert("one group per bracket team", () => groups.Length == Ladder.Teams.Count);
            AddAssert("groups are named after the bracket teams",
                () => groups.Select(g => g.Team.Acronym.Value).SequenceEqual(Ladder.Teams.Select(t => t.Acronym.Value)));

            // The front row belongs to the random phase, so this screen starts with it empty but showing its
            // positions: one per tier, which is why the limit cannot go below the size of the front row.
            AddAssert("every group starts empty", () => groups.All(g => g.Team.Players.Count == 0));
            AddAssert("the tier positions are shown anyway", () => groups.All(g => g.Team.Players.Count == 0 && DraftInfo.PlayerOfTier(g.Team, 1) == null));
            AddAssert("a group shows three tier positions and the rest members",
                () => groups.All(g => g.MemberSlots.Count == DraftInfo.DEFAULT_MEMBERS_PER_TEAM));

            // Clicking the box does not aim at a position: the player takes the first free member position, and
            // the tier positions are never touched.
            AddStep("draft into the first group", () => draftInto(groupAt(0)));

            AddUntilStep("group has one member", () => groupAt(0).Team.Players.Count == 1);
            AddAssert("pool shrunk by one", () => pool.Length == pool_players - 1);
            AddAssert("the player took a member position", () => DraftInfo.DraftedMembersOf(groupAt(0).Team).Count() == 1);
            AddAssert("no tier position was touched", () => groupAt(0).Team.Players.All(p => p.Tier == DraftInfo.NO_TIER));
            // The box has to show the member on its own, which is what the roster binding is for: only a
            // position holding somebody carries a name, so an empty one means the box stopped following.
            AddStep("collect garbage", collectGarbage);
            AddUntilStep("the new member is shown in the box",
                () => groupAt(0).MemberSlots.Count(s => s.ChildrenOfType<MarqueeContainer>().Any()) == 1);

            // The member positions are the limit minus the tier positions, so with the default of four a group
            // can hold exactly one.
            AddStep("try to draft into the full group", () => draftInto(groupAt(0)));

            AddAssert("the group refused the second member", () => groupAt(0).Team.Players.Count == 1);
            AddAssert("the refused player stayed in the pool", () => pool.Length == pool_players - 1);

            // A right click is a click here because the host makes it one, so the test has to be driving the same
            // setup to be proving anything. Both ends are asserted rather than assumed: an input manager which
            // quietly presses instead is how a right click ends up working in the client and dead in the suite.
            AddAssert("the test input manager is driven by clicks for the right button",
                () => InputManager.GetButtonEventManagerFor(MouseButton.Right).EnableClick);
            AddAssert("and the host turns them on too", () =>
            {
                using (var manager = new TournamentInputManager())
                    return manager.GetButtonEventManagerFor(MouseButton.Right).EnableClick;
            });

            // Right clicking a drafted member hands them back.
            AddStep("right click the member position", () =>
            {
                moveMouseTo(groupAt(0).MemberSlots[DraftInfo.FRONT_ROW_TIERS]);
                InputManager.Click(MouseButton.Right);
            });

            AddUntilStep("player returned to the pool", () => pool.Length == pool_players);
            AddAssert("group empty again", () => groupAt(0).Team.Players.Count == 0);

            // Standing in for the random phase, which is the only thing that places a front-row player. The two
            // screens share one session, so a player drawn on the other screen is exactly this.
            AddStep("draw a tier one player into the first group", drawFrontRowPlayer);

            AddUntilStep("the tier position is filled", () => DraftInfo.PlayerOfTier(groupAt(0).Team, 1) != null);
            AddAssert("the group has one player", () => groupAt(0).Team.Players.Count == 1);

            // The front row is not this screen's to remove, so right-clicking it is refused, and it is not
            // filled by this screen either, which is what makes the two screens describe one roster.
            AddStep("right click the tier position", () =>
            {
                moveMouseTo(groupAt(0).MemberSlots[0]);
                InputManager.Click(MouseButton.Right);
            });

            AddAssert("the front row player was not removed", () => groupAt(0).Team.Players.Count == 1);
            AddAssert("and did not go back to the pool", () => pool.Length == pool_players);

            AddStep("select a pool player", selectPoolPlayer);
            AddAssert("a player is selected", () => pool.Any(p => p.Selected.Value));
            AddStep("draft into the group again", () => click(groupAt(0)));

            AddUntilStep("the member joined the front row player", () => groupAt(0).Team.Players.Count == 2);
            AddAssert("the other group was not used", () => groupAt(1).Team.Players.Count == 0);
            AddAssert("the player left the pool", () => pool.Length == pool_players - 1);
            AddAssert("the tier position still holds the drawn player",
                () => groupAt(0).Team.Players.Count(p => p.Tier != DraftInfo.NO_TIER) == 1);

            AddStep("write results", () => button(@"Write results").TriggerClick());

            AddAssert("roster written to the bracket", () => Ladder.Teams.Sum(t => t.Players.Count) == 2);
            AddAssert("draft state persisted", () => storage.Exists(DraftSession.DRAFT_FILENAME));
            AddAssert("draft pool still persisted", () => storage.Exists(DraftSession.PLAYER_LIST_FILENAME));
        }

        /// <summary>
        /// The pool is only ever filled on request: opening the screen, or reloading it, must not replace what
        /// someone is in the middle of drafting from. An empty pool explains itself instead, meaning that the
        /// input still to be set up, or a draft that has run out of players.
        /// </summary>
        [Test]
        public void TestEmptyPoolExplainsItself()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);

            // Taken away for good, so that the pool could not fill itself even if it tried to.
            AddStep("delete the player list", () => storage.Delete(DraftSession.PLAYER_LIST_FILENAME));
            AddStep("reload", reload);

            AddAssert("the pool stayed empty", () => pool.Length == 0);
            AddAssert("groups still shown", () => groups.Length > 0);
            AddAssert("the pool asks for its input file",
                () => poolWarning.Text == DraftStrings.PoolEmptyWarning(DraftSession.PLAYER_LIST_FILENAME));
            AddUntilStep("the warning is laid out", () => poolWarning.DrawWidth > 0 && poolWarningContainer.DrawWidth > 0);
            // The box is sized by its text, so the placeholder has to stay short enough for the narrow column.
            AddAssert("the warning fits the pool column", () => poolWarning.DrawWidth <= poolWarningContainer.DrawWidth);

            // Fetching with nothing to read reports the file itself, on top of the pool placeholder.
            AddStep("fetch players", fetchPlayers);
            AddAssert("the failure names the file it needs",
                () => this.ChildrenOfType<WarningBox>().Any(b => b.Text == DraftStrings.PlayerListMissingWarning(DraftSession.PLAYER_LIST_FILENAME)));

            AddStep("restore the player list", writePlayerList);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);
            AddAssert("every warning is gone", () => !this.ChildrenOfType<WarningBox>().Any());

            // The member positions are the limit minus the tier positions, so the limit has to be widened to
            // empty the pool into the two groups.
            AddStep("raise the member limit to six", () => memberLimitBox.Current.Value = 2 * DraftInfo.FRONT_ROW_TIERS);

            draftWholePool();

            AddUntilStep("pool emptied", () => pool.Length == 0);
            AddAssert("the pool went into both groups", () => groups.All(g => g.Team.Players.Count > 0));
            AddAssert("the draft reads as finished",
                () => poolWarning.Text == DraftStrings.NoPlayerWarning);
        }

        /// <summary>
        /// A player list which names a front row and nothing past it leaves the draft with nobody to draft, and
        /// says so rather than looking like a list which has not been set up.
        /// </summary>
        [Test]
        public void TestFrontRowOnlyPool()
        {
            AddStep("write a front row only player list", writeFrontRowOnlyPlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("the front row was read", () => session.Draft.Candidates.Count > 0);
            AddAssert("every player was classified into the first tier",
                () => session.Draft.Candidates.All(p => p.Tier == 1));
            AddAssert("nothing is offered to the draft", () => pool.Length == 0);
            AddAssert("the pool asks for more players",
                () => poolWarning.Text == DraftStrings.FrontRowOnlyWarning(DraftSession.PLAYER_LIST_FILENAME));
        }

        /// <summary>
        /// The member limit is a setting of the draft rather than of the bracket: lowering it has to hand the
        /// members which no longer fit back to the pool instead of dropping them.
        /// </summary>
        [Test]
        public void TestMemberLimit()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);
            AddAssert("limit starts at the default", () => memberLimitBox.Current.Value == DraftInfo.DEFAULT_MEMBERS_PER_TEAM);

            AddStep("draft a player into the first group", () => draftInto(groupAt(0)));
            AddUntilStep("group has one member", () => groupAt(0).Team.Players.Count == 1);

            // The limit covers the front row too, so the smallest one it can be set to leaves no member position
            // at all and the member already there has to go back to the pool.
            AddStep("lower the member limit to the front row", () => memberLimitBox.Current.Value = DraftInfo.MIN_MEMBERS_PER_TEAM);

            AddUntilStep("group trimmed to the limit", () => groupAt(0).Team.Players.Count == 0);
            AddUntilStep("surplus returned to the pool", () => pool.Length == pool_players);
            AddAssert("the slots followed the limit", () => groupAt(0).MemberSlots.Count == DraftInfo.MIN_MEMBERS_PER_TEAM);

            // Raising it again gives the group its empty slots back without inventing members.
            AddStep("raise the member limit to five", () => memberLimitBox.Current.Value = 5);

            AddUntilStep("slots follow the limit", () => groupAt(0).MemberSlots.Count == 5);
            AddAssert("group keeps its members", () => groupAt(0).Team.Players.Count == 0);

            // The limit belongs to the draft, so it has to come back with it.
            AddStep("reload", reload);
            AddUntilStep("limit restored", () => memberLimitBox.Current.Value == 5);
            AddAssert("reloaded slots follow the limit", () => groupAt(0).MemberSlots.Count == 5);
        }

        /// <summary>
        /// Rebuilding the group boxes must not take the draft's settings with it. A group shows the member limit,
        /// and since disposing a drawable unbinds the bindables it keeps in fields, a group holding the setting
        /// itself would unbind it for everyone as soon as that group is thrown away. Then the control panel is
        /// silently disconnected from the draft.
        /// </summary>
        [Test]
        public void TestSettingsSurviveAGroupRebuild()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);

            // Both of these rebuild every group box.
            AddStep("reload", reload);
            AddStep("raise the group count to six", () => groupCountBox.Current.Value = 6);

            AddUntilStep("six groups", () => groups.Length == 6);
            AddAssert("every group shows the default slots", () => groups.All(g => g.MemberSlots.Count == DraftInfo.DEFAULT_MEMBERS_PER_TEAM));

            AddStep("lower the member limit to the front row", () => memberLimitBox.Current.Value = DraftInfo.MIN_MEMBERS_PER_TEAM);

            AddUntilStep("every group followed the limit", () => groups.All(g => g.MemberSlots.Count == DraftInfo.MIN_MEMBERS_PER_TEAM));
            AddAssert("the limit is still the slider's", () => memberLimitBox.Current.Value == DraftInfo.MIN_MEMBERS_PER_TEAM);

            AddStep("raise the group count back to two", () => groupCountBox.Current.Value = 2);

            AddUntilStep("two groups", () => groups.Length == 2);
            AddAssert("the groups still follow the limit", () => groups.All(g => g.MemberSlots.Count == DraftInfo.MIN_MEMBERS_PER_TEAM));
        }

        /// <summary>
        /// Every position of a group is the same size and they share one listing which wraps two to a line: the
        /// front row leads it and the drafted members follow, told apart by the badge in the cell rather than by
        /// the layout. Sharing the listing is what keeps a group short enough for several of them to be on
        /// screen at once.
        /// </summary>
        [Test]
        public void TestSlotsLayout()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);

            AddStep("draft a player into the first group", () => draftInto(groupAt(0)));
            AddUntilStep("group has one member", () => groupAt(0).Team.Players.Count == 1);

            // Three tier positions and three member positions, so the listing can be seen to wrap.
            AddStep("raise the member limit to six", () => memberLimitBox.Current.Value = 2 * DraftInfo.FRONT_ROW_TIERS);

            AddUntilStep("cells laid out", () => groupAt(0).MemberSlots.All(c => c.DrawWidth > 0));

            AddAssert("the front row leads the listing",
                () => groupAt(0).SlotTiers.Take(DraftInfo.FRONT_ROW_TIERS).SequenceEqual(Enumerable.Range(1, DraftInfo.FRONT_ROW_TIERS)));
            AddAssert("the rest of the listing belongs to the draft",
                () => groupAt(0).SlotTiers.Skip(DraftInfo.FRONT_ROW_TIERS).All(t => t == DraftInfo.NO_TIER));

            AddAssert("every position is the same width", () => allCellsAreTheSameWidth(groupAt(0)));
            AddAssert("the first two positions share a line", () => Math.Abs(cellTop(0) - cellTop(1)) < 0.5f);
            AddAssert("the second is to the right of the first", () => cellLeft(1) > cellLeft(0));
            AddAssert("the third position starts the next line", () => cellTop(2) > cellTop(1));

            AddAssert("every position of the roster is shown", () => groupAt(0).MemberSlots.Count == 2 * DraftInfo.FRONT_ROW_TIERS);
            // Six positions two to a line is three lines, which is what the front row sharing the listing buys.
            AddAssert("a six player roster is three lines tall", () => groupAt(0).Height < 130);
            AddAssert("the drafted member scrolls its name",
                () => groupAt(0).MemberSlots.Count(s => s.ChildrenOfType<MarqueeContainer>().Any()) == 1);
            AddAssert("the scrolling name is clipped to its cell",
                () => groupAt(0).MemberSlots[DraftInfo.FRONT_ROW_TIERS] is Container { Masking: true } cell && cell.ChildrenOfType<MarqueeContainer>().Any());
            AddAssert("pool names scroll as well", () => pool.All(p => p.ChildrenOfType<MarqueeContainer>().Any()));

            // Four columns is what the pool column is sized for, so a panel too many must wrap rather than the
            // listing quietly dropping a column.
            AddUntilStep("the pool is laid out", () => pool.All(p => p.DrawWidth > 0));

            AddAssert("the pool holds four columns", () => poolColumns() == 4);
            AddAssert("the five remaining panels wrap onto a second line", () => pool.Length == pool_players - 1 && poolColumns() < pool.Length);
        }

        /// <summary>
        /// A position is told what to show rather than being built around it, so pointing one which is already
        /// on screen at somebody else has to be enough to make it show them, and taking them off it has to be
        /// enough to empty it again. Nothing has to be told to redraw for either.
        /// </summary>
        [Test]
        public void TestPositionFollowsWhatItIsTold()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);

            AddStep("draft a player into the first group", () => draftInto(groupAt(0)));
            AddUntilStep("group has one member", () => groupAt(0).Team.Players.Count == 1);

            // The last position, which is a member position and so is the empty one in a group holding one member.
            AddStep("point the last position at the member", () => positionAt(0, -1).Player = groupAt(0).Team.Players.Single());

            AddAssert("the position shows them", () => positionAt(0, -1).ChildrenOfType<MarqueeContainer>().Any());

            AddStep("take them off the position", () => positionAt(0, -1).Player = null);

            AddAssert("the position is empty again", () => !positionAt(0, -1).ChildrenOfType<MarqueeContainer>().Any());
        }

        /// <summary>
        /// The boxes are named after the bracket teams they stand for rather than after their position, and that
        /// has to hold for a draft read back from disk as much as for one created on the spot. A file written
        /// before the bracket was set up carries nothing but the fallback letters, and the boxes would go on
        /// showing them with the event's teams sitting right there.
        /// </summary>
        [Test]
        public void TestGroupsAreNamedAfterTheBracketTeams()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);

            AddAssert("the boxes are named after the bracket teams",
                () => groupTitles.SequenceEqual(Ladder.Teams.Select(t => t.FullName.Value)));

            // What the box shows is the team's full name, so a box which had fallen back to the acronym would be
            // caught here as well.
            AddAssert("the box shows the full name rather than the acronym",
                () => groups.All(g => g.Title == g.Team.FullName.Value && g.Title != g.Team.Acronym.Value));

            AddStep("persist a draft named after its positions", () =>
            {
                for (int i = 0; i < session.Draft.Teams.Count; i++)
                {
                    session.Draft.Teams[i].FullName.Value = DraftInfo.GroupNameForPosition(i);
                    session.Draft.Teams[i].Acronym.Value = DraftInfo.GroupNameForPosition(i);
                }

                session.Save();
            });

            AddStep("reload", reload);

            AddUntilStep("the names came back from the bracket",
                () => groupTitles.SequenceEqual(Ladder.Teams.Select(t => t.FullName.Value)));

            // A box the bracket has no team for is the only one which legitimately falls back to its position,
            // which is what raising the count creates.
            AddStep("raise the group count to four", () => groupCountBox.Current.Value = 4);

            AddUntilStep("four groups", () => groups.Length == 4);
            AddAssert("the extra boxes fall back to their position",
                () => groupTitles.SequenceEqual(Ladder.Teams.Select(t => t.FullName.Value).Concat(new[] { "C", "D" })));
        }

        /// <summary>
        /// The control panel offers the save button, and pressing it writes the draft alongside the bracket.
        /// </summary>
        [Test]
        public void TestManualSave()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("forget the persisted draft", forgetPersistedDraft);
            AddStep("reload", reload);
            AddStep("fetch players", fetchPlayers);

            AddUntilStep("pool populated", () => pool.Length == pool_players);

            AddStep("draft a player into the first group", () => draftInto(groupAt(0)));
            AddUntilStep("group has one member", () => groupAt(0).Team.Players.Count == 1);

            // The draft is written as it changes, so the file is taken away to show that saving puts it back.
            AddStep("delete the persisted draft", () => storage.Delete(DraftSession.DRAFT_FILENAME));
            AddAssert("persisted draft gone", () => !storage.Exists(DraftSession.DRAFT_FILENAME));

            AddStep("press save", () => saveButton.Action?.Invoke());

            AddAssert("draft written", () => storage.Exists(DraftSession.DRAFT_FILENAME));
            AddAssert("persisted state matches the draft", () => readPersistedDraft().Teams[0].Players.Count == 1);
        }

        private void forgetPersistedDraft()
        {
            if (storage.Exists(DraftSession.DRAFT_FILENAME))
                storage.Delete(DraftSession.DRAFT_FILENAME);
        }

        private DraftInfo readPersistedDraft()
        {
            using (var stream = storage.GetStream(DraftSession.DRAFT_FILENAME, FileAccess.Read, FileMode.Open))
            using (var reader = new StreamReader(stream))
                return JsonConvert.DeserializeObject<DraftInfo>(reader.ReadToEnd())!;
        }

        /// <summary>
        /// Write a list which names a full front row followed by the players this screen drafts. Invalid lines
        /// are expected to be skipped rather than failing the whole file, and they do not shift the tiers: those
        /// follow the order of the valid IDs only.
        /// </summary>
        private void writePlayerList()
        {
            using (var stream = storage.CreateFileSafely(DraftSession.PLAYER_LIST_FILENAME))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine();
                writer.WriteLine("not a user id");

                for (int i = 0; i < DraftInfo.FRONT_ROW_SIZE + pool_players; i++)
                    writer.WriteLine(3 + i);
            }
        }

        /// <summary>
        /// Write a list which names only part of the front row, i.e. nothing the draft could draft.
        /// </summary>
        private void writeFrontRowOnlyPlayerList()
        {
            using (var stream = storage.CreateFileSafely(DraftSession.PLAYER_LIST_FILENAME))
            using (var writer = new StreamWriter(stream))
            {
                for (int i = 0; i < DraftInfo.PLAYERS_PER_TIER; i++)
                    writer.WriteLine(3 + i);
            }
        }

        /// <summary>
        /// Take the first player of the first tier out of the pool and into the first group.
        /// </summary>
        private void drawFrontRowPlayer()
        {
            TournamentUser player = session.Draft.Candidates.First(p => p.Tier == 1);

            Assert.That(session.Draft.Assign(player, session.Draft.Teams[0]), Is.True);
        }

        /// <summary>
        /// Draft the entire pool into the groups, alternating between the two, one click per step so that each
        /// one lands on a settled layout.
        /// </summary>
        private void draftWholePool()
        {
            for (int i = 0; i < pool_players; i++)
            {
                // The target has to be taken into a local of the loop body. A deferred step which closes over
                // the loop counter itself reads that counter's final value once the step runs, which would send
                // every draft to the same group, and once that group is full, silently drop the rest.
                int target = i % 2;

                AddStep($@"select pool player {i + 1}", selectPoolPlayer);
                AddStep($@"draft pool player {i + 1}", () => click(groupAt(target)));
            }
        }

        private void draftInto(DraftTeamGroup group)
        {
            selectPoolPlayer();
            click(group);
        }

        /// <summary>
        /// Select the player the next placement will use. Clicking the selected panel again clears the selection
        /// rather than keeping it, so an already selected one is never clicked a second time.
        /// </summary>
        private void selectPoolPlayer() => click(pool.First(p => !p.Selected.Value));

        private void reload() => button(@"Reload").TriggerClick();

        private void fetchPlayers() => button(@"Fetch players").TriggerClick();

        /// <summary>
        /// Force a collection, so that anything holding a link only weakly lets go of what it was following.
        /// </summary>
        private static void collectGarbage()
        {
            GC.Collect();
            GC.Collect();
        }

        private TourneyButton button(string name) => this.ChildrenOfType<TourneyButton>().Single(b => b.Name == name);

        private OsuButton saveButton => this.ChildrenOfType<OsuButton>().Single(b => b.Name == @"Save");

        private FormSliderBar<int> memberLimitBox => this.ChildrenOfType<FormSliderBar<int>>().Single(b => b.Name == @"Members per team");

        private FormSliderBar<int> groupCountBox => this.ChildrenOfType<FormSliderBar<int>>().Single(b => b.Name == @"Group count");

        /// <summary>
        /// The placeholder the pool falls back to when there is nothing in it.
        /// </summary>
        private WarningBox poolWarning => poolWarningContainer.ChildrenOfType<WarningBox>().Single();

        private Container poolWarningContainer => this.ChildrenOfType<Container>().Single(c => c.Name == @"Draft pool warning");

        private DraftUserPanel[] pool => this.ChildrenOfType<DraftUserPanel>().ToArray();

        private DraftTeamGroup[] groups => this.ChildrenOfType<DraftTeamGroup>().ToArray();

        private DraftTeamGroup groupAt(int index) => groups.ElementAt(index);

        /// <summary>
        /// A position of the group at <paramref name="groupIndex"/>. A negative <paramref name="slotIndex"/>
        /// counts back from the end of the group's listing.
        /// </summary>
        private PlayerSlotCard positionAt(int groupIndex, int slotIndex)
        {
            var slots = groups.ElementAt(groupIndex).MemberSlots.OfType<PlayerSlotCard>().ToArray();

            return slotIndex >= 0 ? slots[slotIndex] : slots[^(-slotIndex)];
        }

        /// <summary>
        /// The names the group boxes are showing, in team order.
        /// </summary>
        private string[] groupTitles => groups.Select(g => g.Title).ToArray();

        /// <summary>
        /// The vertical position of a position in screen space, which is what tells two of them apart from being
        /// on different lines.
        /// </summary>
        private float cellTop(int index) => groupAt(0).MemberSlots[index].ScreenSpaceDrawQuad.TopLeft.Y;

        private float cellLeft(int index) => groupAt(0).MemberSlots[index].ScreenSpaceDrawQuad.TopLeft.X;

        /// <summary>
        /// Whether every position of <paramref name="group"/> is the same width, which is what makes one listing
        /// of them line up.
        /// </summary>
        private static bool allCellsAreTheSameWidth(DraftTeamGroup group)
            => group.MemberSlots.All(s => Math.Abs(s.DrawWidth - group.MemberSlots[0].DrawWidth) < 0.01f);

        /// <summary>
        /// How many panels share the topmost line of the pool, which is the number of columns it holds.
        /// </summary>
        private int poolColumns()
        {
            float top = pool.Min(p => p.ScreenSpaceDrawQuad.TopLeft.Y);

            return pool.Count(p => Math.Abs(p.ScreenSpaceDrawQuad.TopLeft.Y - top) < 0.5f);
        }

        private void click(Drawable drawable)
        {
            moveMouseTo(drawable);
            InputManager.Click(MouseButton.Left);
        }

        private void moveMouseTo(Drawable drawable) => InputManager.MoveMouseTo(drawable);
    }
}
