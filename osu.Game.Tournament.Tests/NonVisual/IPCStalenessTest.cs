// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Tests.NonVisual
{
    [TestFixture]
    public class IPCStalenessTest
    {
        [Test]
        public void TestNoUpdateTime()
        {
            // nothing has been received at all, which is reported separately from being stale.
            Assert.That(IPCStatusDisplay.IsStale(null, DateTime.UtcNow), Is.False);
        }

        [Test]
        public void TestRecentUpdate()
        {
            DateTime now = DateTime.UtcNow;

            Assert.That(IPCStatusDisplay.IsStale(now, now), Is.False);
            Assert.That(IPCStatusDisplay.IsStale(now - TimeSpan.FromSeconds(1), now), Is.False);
            Assert.That(IPCStatusDisplay.IsStale(now - IPCStatusDisplay.STALE_THRESHOLD, now), Is.False);
        }

        [Test]
        public void TestOutdatedUpdate()
        {
            DateTime now = DateTime.UtcNow;

            Assert.That(IPCStatusDisplay.IsStale(now - IPCStatusDisplay.STALE_THRESHOLD - TimeSpan.FromSeconds(1), now), Is.True);
            Assert.That(IPCStatusDisplay.IsStale(now - TimeSpan.FromMinutes(5), now), Is.True);
        }

        [Test]
        public void TestFutureUpdate()
        {
            // a write time in the future (clock skew) should not be reported as stale.
            DateTime now = DateTime.UtcNow;

            Assert.That(IPCStatusDisplay.IsStale(now + TimeSpan.FromMinutes(1), now), Is.False);
        }
    }
}
