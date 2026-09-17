// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.RandomPick;
using osu.Game.Tournament.Screens.RandomPick.Components;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneRandomPickScreen : TournamentScreenTestScene
    {
        /// <summary>
        /// How many players the player list names past the front row, i.e. everybody this screen has nothing to
        /// do with but has to leave in the pool for the draft screen.
        /// </summary>
        private const int extra_players = 6;

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private DraftSession session { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new RandomPickScreen()
            });
        }

        [Test]
        public void TestDrawFlow()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("start from a clean draft", resetDraft);
            AddStep("fetch players", fetchPlayers);
            AddStep("collect garbage", collectGarbage);

            AddAssert("one box per team", () => pickerGroups.Length == Ladder.Teams.Count);
            AddAssert("the boxes are named after the teams",
                () => pickerGroups.Select(g => g.Team.Acronym.Value).SequenceEqual(Ladder.Teams.Select(t => t.Acronym.Value)));
            // What the box's header shows is the team's full name, which is what the operator recognizes it by.
            AddAssert("the headers name the teams",
                () => pickerGroups.Select(g => g.Title).SequenceEqual(Ladder.Teams.Select(t => t.FullName.Value)));
            AddAssert("every box starts empty", () => pickerGroups.All(g => g.Team.Players.Count == 0));
            AddAssert("every box shows a position per tier",
                () => pickerGroups.All(g => g.TierSlots.Count == DraftInfo.FRONT_ROW_TIERS));

            AddAssert("the strip holds the first tier", () => strip.PlayerCount == DraftInfo.PLAYERS_PER_TIER);
            AddAssert("the header names the tier being drawn", () => tierHeader.Text == RandomPickStrings.PoolHeader(1));

            drawOnce();

            AddUntilStep("a player was drawn", () => pickerGroups.Sum(g => g.Team.Players.Count) == 1);
            AddAssert("the drawn player took a tier position",
                () => pickerGroups.Any(g => DraftInfo.PlayerOfTier(g.Team, 1) != null));
            // The box has to show the player without being rebuilt, which is what the roster binding is for: the
            // rows are read here rather than the model, because a box which lost its binding keeps showing the
            // roster it was built with. This is a state the model cannot describe.
            AddAssert("the drawn player is shown in their box", () => shownPlayers.Count() == 1);
            AddAssert("the strip dropped them", () => strip.PlayerCount == DraftInfo.PLAYERS_PER_TIER - 1);
            AddUntilStep("the drawn player is announced", () => landedName.Alpha > 0);

            // No box is pointed at, so the draws go to the first box with room in the tier: the second one goes
            // to the other team rather than filling the first team's second tier.
            drawOnce();

            AddUntilStep("every box has a tier one player", () => pickerGroups.All(g => DraftInfo.PlayerOfTier(g.Team, 1) != null));
            AddAssert("each box took exactly one", () => pickerGroups.All(g => g.Team.Players.Count == 1));

            // Tier one is finished for every team, so the screen moves on by itself rather than leaving a strip
            // nobody could draw from.
            AddUntilStep("the screen moved on to the next tier", () => tierHeader.Text == RandomPickStrings.PoolHeader(2));
            AddAssert("the strip was refilled from the next tier", () => strip.PlayerCount == DraftInfo.PLAYERS_PER_TIER);

            // Switching is the operator's override, and it steps through the tiers in order.
            AddStep("switch tier", () => button(@"Switch tier").TriggerClick());
            AddAssert("the switch stepped on", () => tierHeader.Text == RandomPickStrings.PoolHeader(3));

            AddStep("switch tier again", () => button(@"Switch tier").TriggerClick());
            AddAssert("the switch wrapped around", () => tierHeader.Text == RandomPickStrings.PoolHeader(1));

            // Every team already has a tier one player, so a draw from it would lead nowhere, and the strip is
            // left alone rather than landing somebody nobody can take.
            AddStep("try to draw from a finished tier", () => button(@"Start draw").TriggerClick());
            AddAssert("the draw was refused", () => statusText.Text == RandomPickStrings.NoTierSlotWarning(1));
            AddAssert("nothing was drawn", () => pickerGroups.Sum(g => g.Team.Players.Count) == 2);

            // What is drawn here is what the draft screen picks up from: the drawn players have left the pool the
            // two screens share.
            AddAssert("the drawn players left the pool",
                () => session.Draft.Candidates.Count(p => p.Tier == 1) == DraftInfo.PLAYERS_PER_TIER - 2);

            AddStep("write to the bracket", () => button(@"Write to bracket").TriggerClick());

            AddAssert("the front row reached the bracket", () => Ladder.Teams.Sum(t => t.Players.Count) == 2);
            AddAssert("every written player carries their tier",
                () => Ladder.Teams.SelectMany(t => t.Players).All(DraftInfo.IsFrontRow));
            AddAssert("the draw is persisted", () => storage.Exists(DraftSession.DRAFT_FILENAME));
        }

        /// <summary>
        /// A box can be pointed at, and that is where the next draw goes. A box which has already taken the tier
        /// being drawn from cannot be drawn into, which is reported when the draw is asked for rather than
        /// quietly going somewhere else.
        /// </summary>
        [Test]
        public void TestSelectingAGroup()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("start from a clean draft", resetDraft);
            AddStep("fetch players", fetchPlayers);
            AddStep("collect garbage", collectGarbage);

            AddAssert("no box is pointed at to start with", () => pickerGroups.All(g => !g.Selected));

            // The second box, so that a draw landing in it cannot be mistaken for the fallback choosing it.
            clickGroup(1);

            AddAssert("the box took the selection", () => pickerGroups[1].Selected);
            AddAssert("only one box is selected", () => pickerGroups.Count(g => g.Selected) == 1);
            AddAssert("the selection is shown on the box itself",
                () => pickerGroups[1].ChildrenOfType<FormControlBackground>().Single().VisualStyle == VisualStyle.Focused);
            AddAssert("the screen says where the next draw goes",
                () => statusText.Text == RandomPickStrings.SelectInfo(displayNameFor(pickerGroups[1].Team)));

            drawOnce();

            AddUntilStep("the player went into the pointed-at box", () => pickerGroups[1].Team.Players.Count == 1);
            AddAssert("the other box was left alone", () => pickerGroups[0].Team.Players.Count == 0);
            AddAssert("the choice was spent on that draw", () => pickerGroups.All(g => !g.Selected));

            // Clicking the pointed-at box a second time hands the choice back rather than keeping it.
            clickGroup(0);

            AddAssert("the first box took the selection", () => pickerGroups[0].Selected);

            clickGroup(0);

            AddAssert("clicking it again cleared the selection", () => pickerGroups.All(g => !g.Selected));

            // Tier one is still the tier being drawn from, because the first box has not had it yet, which is
            // exactly the case of pointing at a box which cannot take the draw.
            clickGroup(1);

            AddStep("ask for a draw into a box which cannot take it", () => button(@"Start draw").TriggerClick());

            AddAssert("the draw was refused",
                () => statusText.Text == RandomPickStrings.SelectedTierFilledWarning(displayNameFor(pickerGroups[1].Team), 1));
            AddAssert("the strip never started", () => !strip.Scrolling);
            AddAssert("nothing was drawn", () => pickerGroups.Sum(g => g.Team.Players.Count) == 1);
            AddAssert("the choice was kept", () => pickerGroups[1].Selected);

            // Handing the choice back leaves the draw to the first box with room, which is the empty one.
            clickGroup(1);

            AddAssert("nothing is pointed at again", () => pickerGroups.All(g => !g.Selected));

            drawOnce();

            AddUntilStep("the draw fell back to the first box with room", () => pickerGroups[0].Team.Players.Count == 1);
            AddAssert("both boxes are shown to hold their player", () => shownPlayers.Count() == 2);
        }

        /// <summary>
        /// Reset empties the boxes and puts everybody back in the pool, so the draw can be run again from the
        /// beginning without reading the player list a second time.
        /// </summary>
        [Test]
        public void TestReset()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("start from a clean draft", resetDraft);
            AddStep("fetch players", fetchPlayers);
            AddStep("collect garbage", collectGarbage);

            drawOnce();
            AddUntilStep("a player was drawn", () => pickerGroups.Sum(g => g.Team.Players.Count) == 1);
            AddAssert("the drawn player is shown in their box", () => shownPlayers.Count() == 1);

            clickGroup(1);
            AddAssert("a box is pointed at", () => pickerGroups[1].Selected);

            AddStep("reset", () => button(@"Reset").TriggerClick());

            AddUntilStep("every box is empty again", () => pickerGroups.All(g => g.Team.Players.Count == 0));
            AddAssert("no box shows a player", () => !shownPlayers.Any());
            AddAssert("everybody is back in the pool",
                () => session.Draft.Candidates.Count == DraftInfo.FRONT_ROW_SIZE + extra_players);
            AddAssert("the front row kept the tier it was seeded into",
                () => session.Draft.Candidates.Count(p => p.Tier != DraftInfo.NO_TIER) == DraftInfo.FRONT_ROW_SIZE);
            AddAssert("the rest is still the draft's to pick from",
                () => session.Draft.Candidates.Count(p => p.Tier == DraftInfo.NO_TIER) == extra_players);
            AddAssert("the strip is full again", () => strip.PlayerCount == DraftInfo.PLAYERS_PER_TIER);
            AddAssert("the tier being drawn from is the first one again", () => tierHeader.Text == RandomPickStrings.PoolHeader(1));
            AddAssert("the selection went with the boxes it referred to", () => pickerGroups.All(g => !g.Selected));
            AddAssert("the reset is reported", () => statusText.Text == RandomPickStrings.ResetInfo);
        }

        /// <summary>
        /// The player list is the only input this screen has, and fetching is the only thing which reads it. A
        /// file which is not there is reported by name rather than leaving an empty screen to be puzzled over.
        /// </summary>
        [Test]
        public void TestMissingPlayerList()
        {
            AddStep("delete the player list", () => storage.Delete(DraftSession.PLAYER_LIST_FILENAME));
            AddStep("start from a clean draft", resetDraft);
            AddStep("fetch players", fetchPlayers);
            AddStep("collect garbage", collectGarbage);

            AddAssert("the failure names the file it needs",
                () => this.ChildrenOfType<WarningBox>().Any(b => b.Text == DraftStrings.PlayerListMissingWarning(DraftSession.PLAYER_LIST_FILENAME)));
            AddAssert("there is nothing to draw from", () => session.Draft.Candidates.Count == 0);
            AddAssert("and nobody has been drawn", () => !session.Draft.AssignedPlayers.Any());

            AddStep("try to draw from an empty pool", () => button(@"Start draw").TriggerClick());
            AddAssert("the draw never started", () => !strip.Scrolling);
            AddAssert("and nobody was drawn", () => !session.Draft.AssignedPlayers.Any());
        }

        /// <summary>
        /// The pool is read from a file, and the file is only ever read on request: starting the client in the
        /// middle of an event must not rewrite the tiers a draw already used.
        /// </summary>
        [Test]
        public void TestRereadingTheListKeepsWhatWasDrawn()
        {
            AddStep("write the player list", writePlayerList);
            AddStep("start from a clean draft", resetDraft);
            AddStep("fetch players", fetchPlayers);
            AddStep("collect garbage", collectGarbage);

            drawOnce();

            AddUntilStep("a player was drawn", () => pickerGroups.Sum(g => g.Team.Players.Count) == 1);

            AddStep("fetch players again", fetchPlayers);

            AddAssert("the drawn player is still in their box",
                () => pickerGroups.SelectMany(g => g.Team.Players).Count() == 1);
            AddAssert("and did not come back to the pool",
                () => session.Draft.Candidates.Count(p => p.Tier == 1) == DraftInfo.PLAYERS_PER_TIER - 1);
            AddAssert("the whole list is in the pool again, bar the drawn player",
                () => session.Draft.Candidates.Count == DraftInfo.FRONT_ROW_SIZE + extra_players - 1);
        }

        /// <summary>
        /// Start the strip and stop it, which is what lands a player. Landing happens over a few seconds of
        /// animation, so the outcome is asserted by the caller.
        /// </summary>
        private void drawOnce()
        {
            AddStep("start the draw", () => button(@"Start draw").TriggerClick());
            AddUntilStep("the strip is moving", () => strip.Scrolling);
            AddStep("stop the draw", () => button(@"Stop draw").TriggerClick());
        }

        /// <summary>
        /// Click the box at <paramref name="index"/> of the team order, which points the next draw at it, or
        /// clears the choice, if that box is the one already pointed at.
        /// </summary>
        /// <remarks>
        /// The click is a step of its own: two clicks in one step are not separated by a frame, and the second
        /// would land on a box which has not yet taken the first's selection.
        /// </remarks>
        private void clickGroup(int index) => AddStep($"click box {index + 1}", () =>
        {
            InputManager.MoveMouseTo(pickerGroups[index]);
            InputManager.Click(MouseButton.Left);
        });

        /// <summary>
        /// The persisted draft is shared with the other tests in this fixture, so each of them starts from a
        /// clean slate: the file is taken away and the session is re-read from what is left, which is the bracket.
        /// </summary>
        /// <remarks>
        /// The tier on screen does not have to be walked back by hand, since every test gets a screen of its own,
        /// which starts on the first tier.
        /// </remarks>
        private void resetDraft()
        {
            if (storage.Exists(DraftSession.DRAFT_FILENAME))
                storage.Delete(DraftSession.DRAFT_FILENAME);

            session.Reload(Ladder);
        }

        /// <summary>
        /// Write a list which names a full front row followed by players the draft would pick from. This screen
        /// only ever draws from the front row, which is the first <see cref="DraftInfo.FRONT_ROW_SIZE"/> of them.
        /// </summary>
        private void writePlayerList()
        {
            using (var stream = storage.CreateFileSafely(DraftSession.PLAYER_LIST_FILENAME))
            using (var writer = new StreamWriter(stream))
            {
                for (int i = 0; i < DraftInfo.FRONT_ROW_SIZE + 6; i++)
                    writer.WriteLine(3 + i);
            }
        }

        private void fetchPlayers() => button(@"Fetch players").TriggerClick();

        /// <summary>
        /// Force a collection, so that anything holding a link only weakly lets go of what it was following.
        /// </summary>
        /// <remarks>
        /// The boxes follow the draft through bound copies, which are only <em>weakly</em> held by the model they
        /// copy. A box which fails to keep hold of its copy therefore stops following the draft whenever the
        /// collector happens to run seconds after a draw in a real session. Collecting on purpose makes that
        /// difference show up either way.
        /// </remarks>
        private static void collectGarbage()
        {
            GC.Collect();
            GC.Collect();
        }

        private TourneyButton button(string name) => this.ChildrenOfType<TourneyButton>().Single(b => b.Name == name);

        private ScrollingPlayerContainer strip => this.ChildrenOfType<ScrollingPlayerContainer>().Single();

        private PickerGroup[] pickerGroups => this.ChildrenOfType<PickerGroup>().ToArray();

        /// <summary>
        /// The players the boxes are showing, read from the rows themselves rather than from the model.
        /// </summary>
        private IEnumerable<TournamentUser> shownPlayers
            => pickerGroups.SelectMany(g => g.TierSlots.OfType<PickerGroup.PickerPlayerCell>()).Select(cell => cell.Player);

        private static string displayNameFor(TournamentTeam team)
            => string.IsNullOrEmpty(team.FullName.Value) ? team.Acronym.Value : team.FullName.Value;

        private TournamentSpriteText tierHeader => this.ChildrenOfType<TournamentSpriteText>().Single(t => t.Name == @"Tier header");

        private TournamentSpriteText statusText => this.ChildrenOfType<TournamentSpriteText>().Single(t => t.Name == @"Status");

        private TournamentSpriteText landedName => this.ChildrenOfType<TournamentSpriteText>().Single(t => t.Name == @"Landed player");
    }
}
