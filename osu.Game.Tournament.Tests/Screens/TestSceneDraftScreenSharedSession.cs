// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Draft;
using osu.Game.Tournament.Screens.Draft.Components;

namespace osu.Game.Tournament.Tests.Screens
{
    /// <summary>
    /// The draft screen as it comes up on a session which is already in use. The two halves of the selection
    /// share one session and every screen is constructed as the client starts, so the draft is read for whichever
    /// of them comes up first; the other is handed a draft which is already there, without a single change ever
    /// being reported for what is in it.
    /// </summary>
    public partial class TestSceneDraftScreenSharedSession : TournamentScreenTestScene
    {
        /// <summary>
        /// How many players the session is left holding when the screen comes up.
        /// </summary>
        private const int pool_players = 2;

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private DraftSession session { get; set; } = null!;

        /// <summary>
        /// The screen under test, brought up by <see cref="TestShowsTheDraftItWasHanded"/> itself.
        /// </summary>
        private DraftScreen screen = null!;

        /// <summary>
        /// The listing is built from the session as it stands, groups and pool both. Nothing reports any of it
        /// to the screen, so waiting to be told would leave it with nothing. But its headers until something else
        /// happened to touch the draft, which is what reloading by hand does.
        /// </summary>
        [Test]
        public void TestShowsTheDraftItWasHanded()
        {
            // Whatever an earlier test left behind is dropped, so that the draft this screen is handed is the
            // one set up here and nothing else.
            AddStep("drop the persisted draft", forgetPersistedDraft);

            // Stands in for the random phase, which is the half that comes up first: the session is read and
            // filled before the screen below is brought up, and this screen is never told about any of it.
            AddStep("read and fill the session first", fillSession);

            AddStep("bring the draft screen up", () => Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = screen = new DraftScreen()
            }));

            AddUntilStep("it built its groups", () => groups.Any());
            AddAssert("one group per group of the draft", () => groups.Length == session.Draft.Teams.Count);
            AddAssert("each named after the team it stands for",
                () => groups.Select(g => g.Title).SequenceEqual(session.Draft.Teams.Select(t => t.FullName.Value)));
            AddAssert("each showing its positions",
                () => groups.All(g => g.MemberSlots.Count == session.Draft.MembersPerTeam.Value));
            AddAssert("and the pool it was handed", () => pool.Length == pool_players);
        }

        private void forgetPersistedDraft()
        {
            if (storage.Exists(DraftSession.DRAFT_FILENAME))
                storage.Delete(DraftSession.DRAFT_FILENAME);
        }

        private void fillSession()
        {
            session.Reload(Ladder);
            session.Draft.Candidates.AddRange(Enumerable.Range(1, pool_players).Select(id => new TournamentUser { OnlineID = id }));
        }

        private DraftTeamGroup[] groups => screen.ChildrenOfType<DraftTeamGroup>().ToArray();

        private DraftUserPanel[] pool => screen.ChildrenOfType<DraftUserPanel>().ToArray();
    }
}
