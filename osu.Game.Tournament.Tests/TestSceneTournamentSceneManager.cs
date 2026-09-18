// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Draft.Components;
using osu.Game.Tournament.Screens.RandomPick.Components;

namespace osu.Game.Tournament.Tests
{
    public partial class TestSceneTournamentSceneManager : TournamentTestScene
    {
        [Resolved]
        private DraftSession session { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new TournamentSceneManager());
        }

        /// <summary>
        /// Every screen is constructed as the client starts, and the two halves of the draft share one session:
        /// the draft is read for whichever of them comes up first, so the other is handed one which is already
        /// in place and is never told about what is in it. It still has to show groups and pool both without
        /// the operator having to reload the screen by hand.
        /// </summary>
        [Test]
        public void TestScreensShowWhatTheyWereHandedAtStartup()
        {
            AddUntilStep("the draft screen built its groups", () => this.ChildrenOfType<DraftTeamGroup>().Any());
            AddAssert("as many groups as the draft holds",
                () => this.ChildrenOfType<DraftTeamGroup>().Count() == session.Draft.Teams.Count);
            AddAssert("each of them showing its positions",
                () => this.ChildrenOfType<DraftTeamGroup>().All(g => g.MemberSlots.Count == session.Draft.MembersPerTeam.Value));

            // The other half of the selection builds its boxes from the session as it finds it rather than from
            // being told about it. It is covered here so that neither half can come to depend on the order the
            // two of them happen to load in.
            AddAssert("the random phase built a box per group as well",
                () => this.ChildrenOfType<PickerGroup>().Count() == session.Draft.Teams.Count);
        }
    }
}
