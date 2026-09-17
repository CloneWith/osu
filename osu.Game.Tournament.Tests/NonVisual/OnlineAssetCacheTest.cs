// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Tests;
using osu.Game.Tournament.Caching;

namespace osu.Game.Tournament.Tests.NonVisual
{
    public partial class OnlineAssetCacheTest
    {
        private const string asset_url = @"https://a.ppy.sh/1234";

        /// <summary>
        /// A 1x1 PNG, which is all the texture loader needs to produce a real texture.
        /// </summary>
        private static readonly byte[] png_data = Convert.FromBase64String(@"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

        /// <summary>
        /// <c>Sprite</c> disposes the texture it was handed, and <c>LargeTextureStore</c> counts every wrapper it
        /// hands out as one reference. Reusing a single wrapper for two callers therefore let the first of them
        /// give up the reference on behalf of both. At this time the store purged the texture and served a released
        /// texture for further requests for that URL, causing exceptions.
        /// </summary>
        [Test]
        public void TestCallersAreGivenOwnTextures()
        {
            using HeadlessGameHost host = new CleanRunHeadlessGameHost();

            try
            {
                startHost(host);

                var store = new StubOnlineStore(png_data);

                using var storage = new TemporaryNativeStorage($@"{nameof(OnlineAssetCacheTest)}-{Guid.NewGuid()}");
                using var cache = new OnlineAssetCache(host, storage, store);

                Texture first = cache.Get(asset_url);
                Texture second = cache.Get(asset_url);

                Assert.That(first, Is.Not.Null, @"the asset was never loaded");
                Assert.That(second, Is.Not.Null);
                Assert.That(second, Is.Not.SameAs(first), @"both callers received the same texture");

                // This is what a sprite does when it is disposed, or handed a different texture.
                first.Dispose();

                Assert.That(second.Available, Is.True, @"releasing one caller's texture also released another caller's");

                second.Dispose();
            }
            finally
            {
                host.Exit();
            }
        }

        /// <summary>
        /// The reference the cache keeps is what stops the texture store from purging the texture once the last
        /// sprite using it is gone. Without it every revisit would fall back to the disk, and a cold disk would
        /// fall back to the network.
        /// </summary>
        [Test]
        public void TestTextureStaysCachedAfterLastCallerReleases()
        {
            using HeadlessGameHost host = new CleanRunHeadlessGameHost();

            try
            {
                startHost(host);

                var store = new StubOnlineStore(png_data);

                using var storage = new TemporaryNativeStorage($@"{nameof(OnlineAssetCacheTest)}-{Guid.NewGuid()}");
                using var cache = new OnlineAssetCache(host, storage, store);

                cache.Get(asset_url).Dispose();

                // Nothing holds a texture of its own at this point, so the only thing keeping the asset in the
                // texture store's cache is the reference the cache itself keeps.
                Texture revived = cache.Get(asset_url);

                Assert.That(revived, Is.Not.Null);
                Assert.That(revived.Available, Is.True, @"the texture was purged while the cache still held it");
                Assert.That(store.Requests, Is.EqualTo(1), @"the asset was fetched again after the last caller let go");

                revived.Dispose();
            }
            finally
            {
                host.Exit();
            }
        }

        /// <summary>
        /// The cache is reached from whichever thread happens to be loading a screen, so several lookups of one
        /// asset can be in flight at once. Each of them still has to be served a texture of its own, and between
        /// them, they must leave behind exactly one reference for the cache, rather than one per caller or none
        /// at all.
        /// </summary>
        /// <remarks>
        /// The lookups deliberately run on plain threads rather than pool threads: the texture store makes a
        /// second lookup of an asset already being loaded wait on the first, and that wait is refused on a pool
        /// thread (see <c>TaskExtensions.WaitSafely</c>), which is not what is being tested here.
        /// </remarks>
        [Test]
        public void TestConcurrentLookupsGetOwnTextures()
        {
            using HeadlessGameHost host = new CleanRunHeadlessGameHost();

            try
            {
                startHost(host);

                var store = new StubOnlineStore(png_data);

                using var storage = new TemporaryNativeStorage($@"{nameof(OnlineAssetCacheTest)}-{Guid.NewGuid()}");
                using var cache = new OnlineAssetCache(host, storage, store);

                const int lookups = 8;

                var textures = new Texture[lookups];
                var errors = new Exception[lookups];

                // Prime the cache before the concurrent lookups, so that they find it the way a revisit of a
                // screen does: with a reference already held. Without this the burst would only ever exercise
                // the cold path.
                cache.Get(asset_url).Dispose();

                using var ready = new CountdownEvent(lookups);
                using var release = new ManualResetEventSlim();
                var threads = new Thread[lookups];

                for (int i = 0; i < lookups; i++)
                {
                    var lookup = new Lookup
                    {
                        Cache = cache,
                        Url = asset_url,
                        Index = i,
                        Textures = textures,
                        Errors = errors,
                        Ready = ready,
                        Release = release,
                    };

                    threads[i] = new Thread(runLookup)
                    {
                        IsBackground = true,
                    };

                    threads[i].Start(lookup);
                }

                Assert.That(ready.Wait(TimeSpan.FromSeconds(30)), Is.True, @"the concurrent lookups never got going");
                release.Set();

                foreach (Thread thread in threads)
                    Assert.That(thread.Join(TimeSpan.FromSeconds(30)), Is.True, @"a concurrent lookup never finished");

                Assert.That(errors, Is.All.Null, @"a concurrent lookup threw");
                Assert.That(textures, Is.All.Not.Null, @"a concurrent lookup produced no texture");
                Assert.That(textures, Is.Unique, @"two callers were handed the same texture");
                Assert.That(store.Requests, Is.EqualTo(1), @"the asset was downloaded once per caller");

                foreach (Texture texture in textures)
                    texture.Dispose();

                // Every caller has now let go of their texture. The cache's own reference is the only thing left.
                Texture revived = cache.Get(asset_url);

                Assert.That(revived, Is.Not.Null);
                Assert.That(revived.Available, Is.True, @"the cache lost its reference when every caller released theirs");
                Assert.That(store.Requests, Is.EqualTo(1), @"the asset was fetched again after the last caller let go");

                revived.Dispose();
            }
            finally
            {
                host.Exit();
            }
        }

        /// <summary>
        /// One concurrent lookup in <see cref="OnlineAssetCacheTest.TestConcurrentLookupsGetOwnTextures"/>, handed to its
        /// thread as a start parameter rather than captured by it.
        /// </summary>
        /// <remarks>
        /// A closure would reach the test's locals, of which several are disposed once the test is done with
        /// them, and would leave it to a join timeout - i.e. to an already failed test - to keep a running
        /// thread away from them. Passing everything in keeps the two lifetimes separate instead.
        /// </remarks>
        private class Lookup
        {
            public required OnlineAssetCache Cache { get; init; }

            public required string Url { get; init; }

            public required int Index { get; init; }

            public required Texture[] Textures { get; init; }

            public required Exception[] Errors { get; init; }

            public required CountdownEvent Ready { get; init; }

            public required ManualResetEventSlim Release { get; init; }
        }

        /// <summary>
        /// Runs one concurrent lookup.
        /// </summary>
        private static void runLookup(object? state)
        {
            var lookup = (Lookup)state!;

            try
            {
                // Held back until every lookup is parked here, so that they genuinely overlap instead of
                // queueing up behind one another.
                lookup.Ready.Signal();
                lookup.Release.Wait(TimeSpan.FromSeconds(30));

                lookup.Textures[lookup.Index] = lookup.Cache.Get(lookup.Url);
            }
            catch (Exception e)
            {
                // Recorded rather than left to escape: should this thread outlive the test's rendezvous, an
                // unhandled exception on a plain thread takes the whole test process down with it, which would
                // hide the assertion that failed in the first place.
                lookup.Errors[lookup.Index] = e;
            }
        }

        private static void startHost(GameHost host)
        {
            var game = new TestGame();

            Task.Factory.StartNew(() => host.Run(game), TaskCreationOptions.LongRunning)
                .ContinueWith(t => Assert.Fail($@"Host threw exception {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);

            TournamentHostTest.WaitForOrAssert(() => game.IsLoaded, @"the game host failed to start");
        }

        private partial class TestGame : osu.Framework.Game
        {
        }

        private class StubOnlineStore : IResourceStore<byte[]>
        {
            private readonly byte[] data;

            public int Requests { get; private set; }

            public StubOnlineStore(byte[] data)
            {
                this.data = data;
            }

            public byte[] Get(string name)
            {
                Requests++;
                return data;
            }

            public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(Get(name));

            public Stream GetStream(string name) => new MemoryStream(Get(name));

            public IEnumerable<string> GetAvailableResources() => Array.Empty<string>();

            public void Dispose()
            {
            }
        }
    }
}
