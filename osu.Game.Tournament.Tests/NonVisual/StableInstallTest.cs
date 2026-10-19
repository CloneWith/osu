// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using NUnit.Framework;
using osu.Game.Tournament.IPC;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class StableInstallTest
    {
        private string testDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        public void TestEmptyPathIsNotAnInstall()
        {
            Assert.That(StableInstall.Inspect(null).IsInstall, Is.False);
            Assert.That(StableInstall.Inspect(string.Empty).IsInstall, Is.False);
            Assert.That(StableInstall.Inspect(testDirectory).IsInstall, Is.False);
        }

        [Test]
        public void TestMissingExecutableIsNotAnInstall()
        {
            // a configuration file on its own is not enough, the directory has to actually hold a game.
            writeFile(StableInstall.CONFIG_FILENAME, config(StableInstall.REQUIRED_RELEASE_STREAM));

            Assert.That(StableInstall.Inspect(testDirectory).IsInstall, Is.False);
        }

        [Test]
        public void TestValidInstall()
        {
            createInstall(StableInstall.REQUIRED_RELEASE_STREAM, true);

            StableInstallStatus status = StableInstall.Inspect(testDirectory);

            Assert.That(status.IsInstall, Is.True);
            Assert.That(status.IsCuttingEdge, Is.True);
            Assert.That(status.IsTournamentReady, Is.True);
            Assert.That(status.IsUsable, Is.True);
        }

        [Test]
        public void TestOtherReleaseStream()
        {
            createInstall(@"Stable", true);

            StableInstallStatus status = StableInstall.Inspect(testDirectory);

            Assert.That(status.IsInstall, Is.True);
            Assert.That(status.IsCuttingEdge, Is.False);
            Assert.That(status.IsUsable, Is.False);
            Assert.That(status.ReleaseStream, Is.EqualTo(@"Stable"));
        }

        [Test]
        public void TestMissingReleaseStream()
        {
            // the key may be absent entirely, which is just as unusable as a different stream.
            createInstall(null, true);

            StableInstallStatus status = StableInstall.Inspect(testDirectory);

            Assert.That(status.IsInstall, Is.True);
            Assert.That(status.IsCuttingEdge, Is.False);
            Assert.That(status.ReleaseStream, Is.Null);
        }

        [Test]
        public void TestTournamentFilesMissing()
        {
            createInstall(StableInstall.REQUIRED_RELEASE_STREAM, false);

            StableInstallStatus status = StableInstall.Inspect(testDirectory);

            Assert.That(status.IsInstall, Is.True);
            Assert.That(status.IsCuttingEdge, Is.True);
            Assert.That(status.IsTournamentReady, Is.False);
            Assert.That(status.IsUsable, Is.False);
        }

        [Test]
        public void TestEachTournamentFileIsRequired()
        {
            string[] essentialFiles = [StableInstall.TOURNAMENT_CONFIG_FILENAME, StableInstall.TOURNAMENT_ACCEPTED_FILENAME, StableInstall.IPC_FILENAME];

            foreach (string filename in essentialFiles)
            {
                createInstall(StableInstall.REQUIRED_RELEASE_STREAM, true);
                File.Delete(Path.Combine(testDirectory, filename));

                Assert.That(StableInstall.Inspect(testDirectory).IsTournamentReady, Is.False, $"missing {filename} should be reported");
            }
        }

        /// <summary>
        /// Creates an installation in <see cref="testDirectory"/>.
        /// </summary>
        /// <param name="releaseStream">The update stream to write to the configuration file, or <c>null</c> to omit it.</param>
        /// <param name="tournamentFiles">Whether the files required for tournament mode should be created.</param>
        private void createInstall(string? releaseStream, bool tournamentFiles)
        {
            writeFile(StableInstall.CONFIG_FILENAME, releaseStream == null ? string.Empty : config(releaseStream));
            writeFile(StableInstall.EXECUTABLE_FILENAME, string.Empty);

            if (!tournamentFiles)
                return;

            writeFile(StableInstall.TOURNAMENT_CONFIG_FILENAME, string.Empty);
            writeFile(StableInstall.TOURNAMENT_ACCEPTED_FILENAME, string.Empty);
            writeFile(StableInstall.IPC_FILENAME, string.Empty);
        }

        private static string config(string releaseStream) => $"[ReleaseStream]\n{StableInstall.RELEASE_STREAM_KEY} = {releaseStream}\n";

        private void writeFile(string filename, string content) => File.WriteAllText(Path.Combine(testDirectory, filename), content);
    }
}
