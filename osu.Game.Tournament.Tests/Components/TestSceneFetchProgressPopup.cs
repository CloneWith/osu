// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneFetchProgressPopup : TournamentTestScene
    {
        private FetchProgressPopup progressPopup;
        private int currentCount;

        public TestSceneFetchProgressPopup()
        {
            Add(progressPopup = new FetchProgressPopup());
        }

        [Test]
        public void TestInitialization()
        {
            AddStep(@"Add new popup if not present", () =>
            {
                if (!progressPopup.IsAlive)
                    Add(progressPopup = new FetchProgressPopup());
            });

            AddStep(@"Set status text", () =>
            {
                progressPopup.PromptString = @"Something is coming on the way!";
                progressPopup.StatusString = @"Fapping...";
            });

            AddStep(@"Set long status text", () =>
                progressPopup.StatusString = @"大家好啊，我是说的道理。今天来点大家想看的东西啊");

            AddStep(@"Set total count", () =>
            {
                progressPopup.TotalCount = 10;
                currentCount = 0;
            });
            AddRepeatStep(@"Set current count", () =>
            {
                currentCount++;
                progressPopup.CurrentCount = currentCount;
            }, 10);
        }

        [Test]
        public void TestCompletion()
        {
            AddAssert(@"Popup instance available", () => progressPopup.IsAlive);

            AddStep(@"Mark as completed", () => progressPopup.SetTaskCompleted());
            AddStep(@"Mark as faulted", () => progressPopup.SetTaskCompleted(true));
        }
    }
}
