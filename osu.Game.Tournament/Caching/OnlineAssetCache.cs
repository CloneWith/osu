// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Online;

namespace osu.Game.Tournament.Caching
{
    /// <summary>
    /// Supplies textures for online assets belonging to the tournament client (avatars, user covers and
    /// beatmap set covers), with a memory layer and a disk layer in front of the network.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The memory layer exists because <see cref="LargeTextureStore"/> drops a texture from its own cache as
    /// soon as the last consumer lets go of it (see <c>TextureWithRefCount</c>). That happens every time a
    /// screen is left or a player list is rebuilt, so without holding a reference here the same avatar would be
    /// re-downloaded on every visit.
    /// </para>
    /// <para>
    /// The disk layer lives in <see cref="CACHE_DIRECTORY"/> under the game-wide storage and is keyed by
    /// the hash of the URL. A cached entry is served even when it is old: on a slow connection a slightly
    /// stale avatar beats no avatar, and the network is only consulted when nothing is on disk.
    /// The same reasoning is why a failed download leaves an existing entry alone.
    /// </para>
    /// <para>
    /// This class implements <see cref="IResourceStore{T}"/> explicitly because the texture loader needs to read
    /// raw bytes through it (<see cref="GameHost.CreateTextureLoaderStore"/>) while the rest of the client wants
    /// a <see cref="Texture"/>. The disk write is performed off the requesting thread so that a slow disk never
    /// delays an asset from appearing on screen.
    /// </para>
    /// </remarks>
    public class OnlineAssetCache : IResourceStore<byte[]>
    {
        /// <summary>
        /// The directory, relative to the game-wide storage, that cached assets are written to.
        /// </summary>
        public const string CACHE_DIRECTORY = @"cache/online-assets";

        /// <summary>
        /// The maximum number of textures kept resident in memory.
        /// </summary>
        public const int MAX_MEMORY_ENTRIES = 256;

        /// <summary>
        /// The maximum total size of the on-disk cache, in bytes.
        /// </summary>
        public const long MAX_DISK_BYTES = 256 * 1024 * 1024;

        /// <summary>
        /// On-disk entries which have not been used for this long are removed by <see cref="PruneDiskCache"/>.
        /// </summary>
        public static readonly TimeSpan MAX_DISK_AGE = TimeSpan.FromDays(30);

        private readonly Storage cacheStorage;
        private readonly LargeTextureStore textureStore;
        private readonly IResourceStore<byte[]> onlineStore;

        /// <summary>
        /// The textures kept resident, keyed by URL. Each entry is held purely to keep the reference count of
        /// the texture it wraps above zero, and is never handed to a consumer.
        /// </summary>
        private readonly Dictionary<string, Texture> pins = new Dictionary<string, Texture>();

        private readonly Dictionary<string, long> pinAccessOrder = new Dictionary<string, long>();
        private readonly Lock pinLock = new Lock();

        private long pinAccessCounter;

        /// <param name="host">The game host, used to reach the renderer.</param>
        /// <param name="storage">The game-wide storage the disk cache lives under.</param>
        /// <param name="onlineStore">
        /// Where assets which are not on disk are fetched from. Defaults to a store restricted to osu!'s own
        /// domains; it is a parameter so that a test can supply assets without touching the network.
        /// </param>
        public OnlineAssetCache(GameHost host, Storage storage, IResourceStore<byte[]> onlineStore = null)
        {
            cacheStorage = storage.GetStorageForDirectory(CACHE_DIRECTORY);
            this.onlineStore = onlineStore ?? new TrustedDomainOnlineStore();

            // The texture loader reads raw bytes back through this instance, which is what gives the disk layer
            // its chance to answer before the network is touched.
            textureStore = new LargeTextureStore(host.Renderer, host.CreateTextureLoaderStore(this));
        }

        /// <summary>
        /// Retrieves the texture for the given URL, preferring a resident copy, then the disk cache, finally the network.
        /// </summary>
        /// <param name="url">The URL of the asset. A blank or untrusted URL yields <c>null</c>.</param>
        /// <returns>
        /// The texture, or <c>null</c> if the asset could not be retrieved. The returned texture belongs to the
        /// caller: it is released when the sprite using it is disposed or handed a different texture.
        /// </returns>
        public Texture Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            bool pinned = touchPin(url);

            Texture texture = fetch(url);

            if (texture == null)
                return null;

            if (!texture.Available)
            {
                // Nothing should reach here: the pin below keeps the texture in the texture store's cache, and
                // that store only purges a texture once every consumer is gone. Guarded regardless, because
                // `Sprite.Texture` reads the texture's size the moment it is assigned and a released texture
                // throws when it does, which mid-broadcast is far worse than an asset which does not appear.
                Logger.Log($@"Online asset {url} was served after it had already been released.", level: LogLevel.Important);
                return null;
            }

            if (!pinned)
                pin(url);

            return texture;
        }

        /// <summary>
        /// Retrieves the texture for the given URL from the texture store, reporting an unreadable asset as
        /// <c>null</c> rather than letting it reach the caller.
        /// </summary>
        /// <remarks>
        /// Must not be called with the pin lock held. See the class remarks for why.
        /// </remarks>
        private Texture fetch(string url)
        {
            try
            {
                return textureStore.Get(url);
            }
            catch (Exception e)
            {
                // A corrupt cache entry must not be able to take the client down, so drop it and let the next
                // lookup fetch the asset again.
                Logger.Error(e, $@"Failed to load online asset {url}.");
                deleteCached(url);
                return null;
            }
        }

        /// <summary>
        /// Marks the given URL as recently used if it is pinned, reporting whether it was.
        /// </summary>
        private bool touchPin(string url)
        {
            lock (pinLock)
            {
                if (!pins.ContainsKey(url))
                    return false;

                pinAccessOrder[url] = ++pinAccessCounter;
                return true;
            }
        }

        /// <summary>
        /// Takes a private reference to the given URL's texture, so that the texture store keeps that texture
        /// cached once the caller releases the reference they were handed.
        /// </summary>
        private void pin(string url)
        {
            // A second lookup of the same URL. Performed outside the pin lock for the same reason as the one in
            // `Get`: it reaches into the texture store, which locks internally.
            Texture extra = fetch(url);

            if (extra == null)
                return;

            List<Texture> released;

            lock (pinLock)
            {
                // Another thread may have pinned the same URL while this one was loading. The reference taken
                // here is then surplus, and giving it up leaves the count exactly one above what callers hold.
                if (!pins.TryAdd(url, extra))
                    released = new List<Texture> { extra };
                else
                {
                    pinAccessOrder[url] = ++pinAccessCounter;
                    released = trimPins();
                }
            }

            // Giving up the last reference of a texture runs the texture store's purge path, so it happens here
            // rather than inside the lock.
            if (released == null)
                return;

            foreach (Texture texture in released)
                texture.Dispose();
        }

        /// <summary>
        /// Removes on-disk entries which have aged out or no longer fit the cache budget. Runs in the background.
        /// </summary>
        public void PruneDiskCache()
        {
            Task.Run(() =>
            {
                try
                {
                    var entries = new List<(string file, DateTimeOffset lastUsed, long length)>();

                    foreach (string file in cacheStorage.GetFiles(string.Empty))
                    {
                        var info = new FileInfo(cacheStorage.GetFullPath(file));

                        if (info.Exists)
                            entries.Add((file, info.LastWriteTimeUtc, info.Length));
                    }

                    DateTimeOffset cutoff = DateTimeOffset.UtcNow - MAX_DISK_AGE;
                    long kept = 0;
                    int removed = 0;

                    // Newest first, so the entries worth keeping are accounted for before older ones are judged.
                    foreach (var entry in entries.OrderByDescending(e => e.lastUsed))
                    {
                        if (entry.lastUsed < cutoff || kept + entry.length > MAX_DISK_BYTES)
                        {
                            cacheStorage.Delete(entry.file);
                            removed++;
                            continue;
                        }

                        kept += entry.length;
                    }

                    Logger.Log($@"Online asset cache holds {entries.Count - removed} entries ({kept / 1024 / 1024}MB), removed {removed}.");
                }
                catch (Exception e)
                {
                    // A cache which cannot be pruned is not worth failing over.
                    Logger.Error(e, @"Failed to prune the online asset cache.");
                }
            });
        }

        /// <summary>
        /// Drops the least recently used pins until the cache fits its budget, returning the textures whose
        /// reference was given up.
        /// </summary>
        /// <remarks>
        /// The returned textures must be disposed by the caller once the pin lock has been released. See the
        /// class remarks for why.
        /// </remarks>
        private List<Texture> trimPins()
        {
            List<Texture> released = null;

            while (pins.Count > MAX_MEMORY_ENTRIES)
            {
                string oldest = null;
                long oldestOrder = long.MaxValue;

                foreach (KeyValuePair<string, long> entry in pinAccessOrder.Where(entry => entry.Value < oldestOrder))
                {
                    oldestOrder = entry.Value;
                    oldest = entry.Key;
                }

                if (oldest == null)
                    break;

                // Dropping the pin is what hands the texture back to the texture store, provided no consumer is
                // still holding a wrapper of its own.
                (released ??= new List<Texture>()).Add(pins[oldest]);

                pins.Remove(oldest);
                pinAccessOrder.Remove(oldest);
            }

            return released;
        }

        private byte[] getBytes(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            string path = getCachedPath(url);

            if (path != null)
                return File.ReadAllBytes(path);

            byte[] downloaded = onlineStore.Get(url);

            if (downloaded != null)
                storeToDisk(url, downloaded);

            return downloaded;
        }

        /// <summary>
        /// Returns the path of a cached copy of the given URL, or <c>null</c> if there is none.
        /// </summary>
        private string getCachedPath(string url)
        {
            try
            {
                string path = cacheStorage.GetFullPath(url.ComputeSHA2Hash());

                if (!File.Exists(path))
                    return null;

                // Touching the timestamp is how an entry records that it is still in use, which is what
                // PruneDiskCache sorts on.
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                return path;
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Failed to look up cached online asset {url}.");
                return null;
            }
        }

        private void storeToDisk(string url, byte[] data)
        {
            string path = cacheStorage.GetFullPath(url.ComputeSHA2Hash());

            // The I/O happens off the requesting thread, and goes through a temporary name so that a
            // concurrent or interrupted write can never be read back as a truncated asset.
            Task.Run(() =>
            {
                try
                {
                    string temporary = path + @".tmp";
                    File.WriteAllBytes(temporary, data);
                    File.Move(temporary, path, true);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Failed to cache online asset {url}.");
                }
            });
        }

        private void deleteCached(string url)
        {
            try
            {
                cacheStorage.Delete(url.ComputeSHA2Hash());
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Failed to delete corrupt online asset {url}.");
            }
        }

        #region IResourceStore<byte[]> implementation

        // Implemented explicitly because the public `Get(string)` member above returns a Texture and would
        // otherwise collide with the byte[]-returning member that shares its name.

        byte[] IResourceStore<byte[]>.Get(string name) => getBytes(name);

        Task<byte[]> IResourceStore<byte[]>.GetAsync(string name, CancellationToken cancellationToken)
            => Task.Run(() => getBytes(name), cancellationToken);

        Stream IResourceStore<byte[]>.GetStream(string name)
        {
            byte[] data = getBytes(name);
            return data == null ? null : new MemoryStream(data);
        }

        IEnumerable<string> IResourceStore<byte[]>.GetAvailableResources() => Enumerable.Empty<string>();

        #endregion

        public void Dispose()
        {
            onlineStore.Dispose();
            textureStore.Dispose();
        }
    }
}
